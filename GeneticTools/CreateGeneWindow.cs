using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateGeneWindow : EditorWindow
{
	#region Fields
	public const string ASSETS_FOLDER = "Assets";
	public const float GUI_LABEL_WIDTH = 160;

	private const string SCRIPTABLE_OBJECT_FOLDER_SUFFIX = "/ScriptableObject";
	private const string SCRIPTABLE_OBJECT_SUFFIX = "SObject";
	private const string SCRIPT_EXTENSION = "cs";

	private bool haveScriptableObject = true;
	private bool addNamespace = false;
	private string namespaceName;

	private string scriptsFolderPath = "Assets/Scripts";
	private string scriptsObjectFolderPath = "Assets/Scripts/ScriptableObject";
	private string scriptName;
	private string geneName;
	private string geneSoName;
	private string typeName;
	private bool isDominantBase;
	private string baseMutateChances = "0";
	#endregion

	[MenuItem("Tools/Genetic/Create Gene")]
	public static void Init()
	{
		CreateGeneWindow window = GetWindow<CreateGeneWindow>();
		window.Show();
	}

	private bool IsGeneNameValid(params string[] scriptNames)
	{
		foreach (string name in scriptNames)
		{
			if (Type.GetType((addNamespace ? namespaceName + "." : "") + name) == null)
				continue;

			EditorUtility.DisplayDialog("Abort: could not create scripts", $"A class called {(addNamespace ? namespaceName + "." : "") + name} already exists.", "OK");
			return false;
		}
		return true;
	}

	private string GUIFolderPathField(string path, string name, string description, bool inAssetsFolder = true)
	{
		return GUIPathField(path, name, description, EditorUtility.OpenFolderPanel, inAssetsFolder);
	}

	private string GUIPathField(string path, string name, string description, Func<string, string, string, string> selectPath, bool inAssetsFolder)
	{
		GUILayout.BeginHorizontal();

		EditorGUILayout.LabelField(new GUIContent(name, description), GUILayout.Width(GUI_LABEL_WIDTH));
		bool isGuiEnable = GUI.enabled;
		GUI.enabled = false;
		EditorGUILayout.TextField(path, GUILayout.MinWidth(90));
		GUI.enabled = isGuiEnable;

		if (GUILayout.Button(new GUIContent("...", "Browse to location"), EditorStyles.miniButton, GUILayout.Width(25)))
		{
			string folderPath = !string.IsNullOrWhiteSpace(path) ? System.IO.Path.GetDirectoryName(path) : "";
			string newPath = selectPath(name, folderPath, "");
			if (!string.IsNullOrEmpty(newPath))
			{
				if (!inAssetsFolder)
					path = newPath;
				else if (newPath.Contains(Application.dataPath))
					path = newPath.Substring(Application.dataPath.Length - ASSETS_FOLDER.Length);
				else
					EditorUtility.DisplayDialog("Invalid Path!", "Path isn't in Project's Assets folder", "OK");
			}
		}

		GUILayout.EndHorizontal();

		return path;
	}

	private void DisplayScriptButton()
	{
		bool isNameValid = !string.IsNullOrWhiteSpace(scriptName) && !string.IsNullOrWhiteSpace(typeName);

		if (!isNameValid)
			GUI.enabled = false;

		if (GUILayout.Button("Create scripts"))
			CreateScripts();

		if (!isNameValid)
			GUI.enabled = true;
	}

	private void OnGUI()
	{
		using (new EditorGUI.IndentLevelScope())
		{
			haveScriptableObject = EditorGUILayout.Toggle(new GUIContent("Scriptable Object", "Create a gene with or without Scriptable Object"), haveScriptableObject);

			addNamespace = EditorGUILayout.Toggle(new GUIContent("Add Namespace", "include the scripts into a namespace"), addNamespace);
			if (addNamespace)
			{
				namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
			}

			GUILayout.Label("Path", EditorStyles.largeLabel);
			scriptsFolderPath = GUIFolderPathField(scriptsFolderPath, "Gene Destination Folder", "Select the folder where the gene will be created");
			if (haveScriptableObject)
			{
				GUI.enabled = false;
				scriptsObjectFolderPath = scriptsFolderPath + SCRIPTABLE_OBJECT_FOLDER_SUFFIX;
				scriptsObjectFolderPath = GUIPathField(scriptsObjectFolderPath, "Scriptable Object Gene Destination Folder", "", null, true);
				GUI.enabled = true;
			}

			GUILayout.Label("Names", EditorStyles.largeLabel);
			scriptName = EditorGUILayout.TextField("Name", scriptName);
			GUI.enabled = false;
			geneName = scriptName + "Gene";
			EditorGUILayout.TextField("Gene", geneName);

			geneSoName = scriptName + "GeneSO";
			if (haveScriptableObject)
				EditorGUILayout.TextField("Gene", geneSoName);
			GUI.enabled = true;

			GUILayout.Label("Contain Type", EditorStyles.largeLabel);
			typeName = EditorGUILayout.TextField("Type name", typeName);

			GUILayout.Label("Data", EditorStyles.largeLabel);
			isDominantBase = EditorGUILayout.Toggle("Is dominant by default", isDominantBase);

			baseMutateChances = EditorGUILayout.TextField("Base chances to mutate", baseMutateChances);
		}

		DisplayScriptButton();
	}

	private string GetGeneScript()
	{
		return
			"using UnityEngine;\nusing Genetic;\n\n" +
			(addNamespace ? "namespace " + namespaceName + "\n{\n" : "") +
			"public class " + geneName + " : Gene<" + typeName + ">\n" +
			"{\n" +
			"#region Fields\n" +
			"private " + typeName + " data;\n" +
			"#endregion\n\n" +
			"#region Methods\n" +
			"public " + geneName + "(" + typeName + " data, bool isDominant = " + (isDominantBase ? "true" : "false") + ", float chancesToMutate = " + baseMutateChances + "f)\n{\n" +
			"Encode(data);\nthis.isDominant = isDominant;\nthis.chancesToMutate = chancesToMutate;\n}\n\n" +
			"public override void Encode(" + typeName + " data)\n{\n" +
			"this.data = data;\n}\n\n" +
			"public override " + typeName + " Decode()\n{\n" +
			"return data;\n}\n\n" +
			"public override object FuzeDecode(IGene gene)\n{\n" +
			geneName + " other = (" + geneName + ")gene;\n" +
			"throw new System.NotImplementedException();\n}\n\n" +
			"protected override void Mutate()\n{\n" +
			"throw new System.NotImplementedException();\n}\n\n" +
			"public override IGene Clone()\n{\n" +
			"return new " + geneName + "(this.data, this.isDominant, this.chancesToMutate);\n}\n\n" +
			"public override IGene Replicate()\n{\n" +
			geneName + " other = (" + geneName + ")this.Clone();\n\n" +
			"if (Random.value < this.chancesToMutate)\n" +
			"other.Mutate();\n" +
			"return other;\n}\n" +
			"#endregion\n" +
			"}" +
			(addNamespace ? "\n}" : "");
	}

	private string GetGeneObjectScripts()
	{
		return
			"using UnityEngine;\nusing Genetic;\n\n" +
			(addNamespace ? "namespace " + namespaceName + "\n{\n" : "") +
			"[CreateAssetMenu(menuName = \"Genetic/Gene/" + geneName + "\")]\n" +
			"public class " + geneName + SCRIPTABLE_OBJECT_SUFFIX + " : GeneObject<" + geneName + ">\n{\n" +
			"[SerializeField]\nprivate " + typeName + " data;\n\n" +
			"public override " + geneName + " GetGene()\n{\n" +
			geneName + " gene = new " + geneName + "(data);\n" +
			"return gene;\n}\n}" +
			(addNamespace ? "\n}" : "");
	}

	private void CreateScripts()
	{
		if (!IsGeneNameValid(geneName, geneSoName))
			return;

		string lastScriptPath = scriptsFolderPath;
		scriptsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), scriptsFolderPath);
		string lastSoScriptPath = scriptsObjectFolderPath;
		scriptsObjectFolderPath = Path.Combine(Directory.GetCurrentDirectory(), scriptsObjectFolderPath);

		if (!Directory.Exists(scriptsFolderPath))
			Directory.CreateDirectory(scriptsFolderPath);

		if (!Directory.Exists(scriptsObjectFolderPath) && haveScriptableObject)
			Directory.CreateDirectory(scriptsObjectFolderPath);

		File.WriteAllText(Path.Combine(scriptsFolderPath, geneName + "." + SCRIPT_EXTENSION), GetGeneScript());
		File.WriteAllText(Path.Combine(scriptsObjectFolderPath, geneName + SCRIPTABLE_OBJECT_SUFFIX + "." + SCRIPT_EXTENSION), GetGeneObjectScripts());

		scriptsFolderPath = lastScriptPath;
		scriptsObjectFolderPath = lastSoScriptPath;

		Debug.Log(geneName + " correctly generated");
	}
}
