﻿using System.Linq;
using MyBox.EditorTools;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace MyBox.Internal
{
	[CustomEditor(typeof(AssetsPresetPreprocessBase))]
	public class AssetsPresetPreprocessEditor : Editor
	{
		[MenuItem("Tools/MyBox/Postprocess Preset Tool", false, 50)]
		private static void PingBase()
		{
			var presetBase = MyScriptableObject.LoadAssetsFromResources<AssetsPresetPreprocessBase>().FirstOrDefault();
			if (presetBase == null)
			{
				presetBase = MyScriptableObject.CreateAssetWithFolderDialog<AssetsPresetPreprocessBase>("AssetsPresetPostprocessBase");
			}

			if (presetBase != null)
			{
				EditorGUIUtility.PingObject(presetBase);
				Selection.activeObject = presetBase;
			}
		}

		private Vector2 _scrollPos;
		private GUIStyle _labelStyle;

		private ReorderableCollection _reorderableBase;

		private void OnEnable()
		{
			_labelStyle = new GUIStyle(EditorStyles.label);
			_labelStyle.richText = true;

			_reorderableBase = new ReorderableCollection(serializedObject.FindProperty("Presets"));

			_reorderableBase.CustomDrawerHeight += PresetDrawerHeight;
			_reorderableBase.CustomDrawer += PresetDrawer;
		}

		private void OnDisable()
		{
			_reorderableBase.CustomDrawerHeight -= PresetDrawerHeight;
			_reorderableBase.CustomDrawer -= PresetDrawer;
			_reorderableBase = null;
		}

		private int PresetDrawerHeight(int index)
		{
			return (int) (EditorGUIUtility.singleLineHeight * 2 + 4);
		}

		private void PresetDrawer(SerializedProperty property, Rect rect, int index)

		{
			var properties = new PresetProperties(property);
			DrawPresetColourLine(rect, properties.Preset.objectReferenceValue as Preset);
			rect.width -= 6;
			rect.x += 6;


			EditorGUI.BeginChangeCheck();

			rect.height = EditorGUIUtility.singleLineHeight;
			var labelWidth = 24;
			var betweenFields = 6;

			var firstLineRect = new Rect(rect);
			var flRatio = (rect.width - (labelWidth * 2 + betweenFields)) / 5;
			firstLineRect.width = flRatio * 3;

			EditorGUI.LabelField(firstLineRect, "PC:");
			firstLineRect.x += labelWidth;
			EditorGUI.PropertyField(firstLineRect, properties.PathContains, GUIContent.none);

			firstLineRect.x += firstLineRect.width + betweenFields;
			firstLineRect.width = flRatio * 2;
			EditorGUI.LabelField(firstLineRect, "FT:");
			firstLineRect.x += labelWidth;
			EditorGUI.PropertyField(firstLineRect, properties.TypeOf, GUIContent.none);


			rect.y += EditorGUIUtility.singleLineHeight + 2;
			var secondLineRect = new Rect(rect);
			var slRatio = (rect.width - (labelWidth * 3 + betweenFields * 2)) / 10;

			var halfW = flRatio * 3 / 2 - (labelWidth / 2f) - (betweenFields / 2f);
			secondLineRect.width = halfW;
			EditorGUI.LabelField(secondLineRect, "Pr:");
			secondLineRect.x += labelWidth;
			EditorGUI.PropertyField(secondLineRect, properties.Prefix, GUIContent.none);

			secondLineRect.x += secondLineRect.width + betweenFields;
			secondLineRect.width = halfW;
			EditorGUI.LabelField(secondLineRect, "Po:");
			secondLineRect.x += labelWidth;
			EditorGUI.PropertyField(secondLineRect, properties.Postfix, GUIContent.none);

			secondLineRect.x += secondLineRect.width + betweenFields;
			secondLineRect.width = slRatio * 4;
			secondLineRect.x += labelWidth;
			EditorGUI.PropertyField(secondLineRect, properties.Preset, GUIContent.none);


			if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
		}


		private struct PresetProperties
		{
			public readonly SerializedProperty PathContains;
			public readonly SerializedProperty TypeOf;
			public readonly SerializedProperty Prefix;
			public readonly SerializedProperty Postfix;

			public readonly SerializedProperty Preset;

			public PresetProperties(SerializedProperty baseProperty)
			{
				PathContains = baseProperty.FindPropertyRelative("PathContains");
				TypeOf = baseProperty.FindPropertyRelative("TypeOf");
				Prefix = baseProperty.FindPropertyRelative("Prefix");
				Postfix = baseProperty.FindPropertyRelative("Postfix");
				Preset = baseProperty.FindPropertyRelative("Preset");
			}
		}

		private void DrawPresetColourLine(Rect rect, Preset preset)
		{
			if (preset == null) return;
			var cRect = new Rect(rect);
			cRect.width = 6;
			cRect.height -= 2;

			Color color = MyGUI.Brown;
			var presetType = preset.GetTargetTypeName();
			if (presetType.Contains("Texture")) color = MyGUI.Blue;
			else if (presetType.Contains("Audio")) color = MyGUI.Red;

			MyGUI.DrawColouredRect(cRect, color);
			EditorGUI.LabelField(cRect, GUIContent.none);
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("First match will be applied");
			EditorGUILayout.LabelField("Assets/...<b>[PC:Path Contains]</b>.../", _labelStyle);
			EditorGUILayout.LabelField("<b>[Pr:Prefix]</b>...<b>[Po:Postfix]</b>.<b>[FT:File Type]</b>", _labelStyle);
			EditorGUILayout.Space();

			_scrollPos = GUILayout.BeginScrollView(_scrollPos);

			_reorderableBase.Draw();

			GUILayout.EndScrollView();
		}
	}
}