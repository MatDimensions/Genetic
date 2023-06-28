using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateGenomeWindow : EditorWindow
{
	#region fields
	public const string ASSETS_FOLDER = "Assets";
	public const float GUI_LABEL_WIDTH = 160;

	private const string SCRIPTABLE_OBJECT_FOLDER_SUFFIX = "/ScriptableObject";
	private const string SCRIPTABLE_OBJECT_SUFFIX = "SObject";
	private const string SCRIPT_EXTENSION = "cs";

	private const string GENOME_SUFFIX = "Genome";
	private const string GENOTYPE_SUFFIX = "Genotype";

	private bool haveScriptableObject = true;
	private bool addNamespace = false;
	private string namespaceName;

	private string scriptsFolderPath = "Assets/Scripts";
	private string scriptsObjectFolderPath = "Assets/Scripts/ScriptableObject";
	private string scriptName;
	private string genotypeName;
	private string genomeName;
	private string genomeSoName;
	#endregion

	[MenuItem("Tools/Genetic/Create Genome")]
	public static void Init()
	{
		CreateGenomeWindow window = GetWindow<CreateGenomeWindow>();
		window.Show();
	}

	private bool IsGenomeNameValid(params string[] scriptNames)
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
		bool isNameValid = !string.IsNullOrWhiteSpace(scriptName);

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
			haveScriptableObject = EditorGUILayout.Toggle(new GUIContent("Scriptable Object", "Create a genome with or without Scriptable Object"), haveScriptableObject);

			addNamespace = EditorGUILayout.Toggle(new GUIContent("Add Namespace", "include the scripts into a namespace"), addNamespace);
			if (addNamespace)
			{
				namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
			}

			GUILayout.Label("Path", EditorStyles.largeLabel);
			scriptsFolderPath = GUIFolderPathField(scriptsFolderPath, "Genome Destination Folder", "Select the folder where the gene will be created");
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
			genotypeName = scriptName + GENOTYPE_SUFFIX;
			EditorGUILayout.TextField("Genotype", genotypeName);

			genomeName = scriptName + GENOME_SUFFIX;
			EditorGUILayout.TextField("Genome", genomeName);

			genomeSoName = scriptName + GENOME_SUFFIX + SCRIPTABLE_OBJECT_SUFFIX;
			if (haveScriptableObject)
				EditorGUILayout.TextField("Gene", genomeSoName);
			GUI.enabled = true;
		}

		DisplayScriptButton();
	}

	private string GetGenomeScript()
	{
		return
			"using UnityEngine;\nusing Genetic;\n\n" +
			(addNamespace ? "namespace " + namespaceName + "\n{\n" : "") +
			"public struct " + genotypeName + " : IGenotype\n{\n\n" +
			"public void Setup(object[] data)\n{\n" +
			"throw new System.NotImplementedException();\n}\n\n" +
			"public override string ToString()\n{\n" +
			"throw new System.NotImplementedException();\n}\n" +
			"}\n\n" +
			"public class " + genomeName + " : Genome<" + genotypeName + ">\n{\n" +
			"//Don't use this constructor in your game, this is generated only for reflection use\n" +
			"public " + genomeName + "() : base() { }\n" +
			"public " + genomeName + "(IGene[] genes) : base(genes) { }\n" +
			"public " + genomeName + "(int size, IGene[] genes) : base(size, genes) { }\n" +
			"public " + genomeName + "(IGene[] fatherGenes, IGene[] motherGenes) : base(fatherGenes, motherGenes) { }\n" +
			"public " + genomeName + "(" + genomeName + " father, " + genomeName + " mother) : base(father, mother) { }\n" +
			"}\n" +
			(addNamespace ? "\n}" : "");
	}

	private string GetGenotypeObjectScripts()
	{
		return
			"using UnityEngine;\nusing Genetic;\n\n" +
			(addNamespace ? "namespace " + namespaceName + "\n{\n" : "") +
			(addNamespace ? "\t" : "") + "[CreateAssetMenu(menuName = \"Genetic/Genome/" + genomeName + "\")]\n" +
			(addNamespace ? "\t" : "") + "public class " + genomeSoName + " : GenomeObject<" + genomeName + ", " + genotypeName + "> { }" +
			(addNamespace ? "\n}" : "");
	}

	private void CreateScripts()
	{
		if (!IsGenomeNameValid(genotypeName, genomeName, genomeSoName))
			return;

		string lastScriptPath = scriptsFolderPath;
		scriptsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), scriptsFolderPath);
		string lastSoScriptPath = scriptsObjectFolderPath;
		scriptsObjectFolderPath = Path.Combine(Directory.GetCurrentDirectory(), scriptsObjectFolderPath);

		if (!Directory.Exists(scriptsFolderPath))
			Directory.CreateDirectory(scriptsFolderPath);

		if (!Directory.Exists(scriptsObjectFolderPath) && haveScriptableObject)
			Directory.CreateDirectory(scriptsObjectFolderPath);

		File.WriteAllText(Path.Combine(scriptsFolderPath, genomeName + "." + SCRIPT_EXTENSION), GetGenomeScript());
		File.WriteAllText(Path.Combine(scriptsObjectFolderPath, genomeName + SCRIPTABLE_OBJECT_SUFFIX + "." + SCRIPT_EXTENSION), GetGenotypeObjectScripts());

		scriptsFolderPath = lastScriptPath;
		scriptsObjectFolderPath = lastSoScriptPath;

		Debug.Log(genomeName + " correctly generated");

		//UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
		//EditorUtility.RequestScriptReload();
	}
}
