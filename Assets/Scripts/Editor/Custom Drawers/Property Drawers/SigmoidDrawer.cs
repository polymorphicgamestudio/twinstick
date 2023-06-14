using Codice.CM.Client.Differences.Graphic;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace ShepProject
{


    [CustomPropertyDrawer(typeof(Sigmoid))]
    public class SigmoidDrawer : PropertyDrawer
    {


        Material material;
        public float xMin = -20;
        public float xMax = 20;
        public int pointCount = 30;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float spacer = 2;
            float totalSpace = position.height;
            Rect pos = position;
            EditorGUI.BeginProperty(pos, label, property);

            EditorGUI.PrefixLabel(position, label);
            pos.y += 20;


            SerializedProperty magnitudeField = property.FindPropertyRelative("magnitude");
            SerializedProperty slopeField = property.FindPropertyRelative("slope");
            SerializedProperty verticalOffsetField = property.FindPropertyRelative("verticalOffset");
            SerializedProperty horizontalOffsetField = property.FindPropertyRelative("horizontalOffset");

            pos.height = EditorGUI.GetPropertyHeight(magnitudeField);
            totalSpace += pos.height + spacer;
            EditorGUILayout.PropertyField(magnitudeField);

            pos.y += pos.height + spacer;
            pos.height = EditorGUI.GetPropertyHeight(slopeField);
            totalSpace += pos.height + spacer;
            EditorGUILayout.PropertyField(slopeField);


            pos.y += pos.height + spacer;
            pos.height = EditorGUI.GetPropertyHeight(verticalOffsetField);
            totalSpace += pos.height + spacer;
            EditorGUILayout.PropertyField(verticalOffsetField);

            pos.y += pos.height + spacer;
            pos.height = EditorGUI.GetPropertyHeight(horizontalOffsetField);
            totalSpace += pos.height + spacer;
            EditorGUILayout.PropertyField(horizontalOffsetField);



            totalSpace += 170;
            GUILayout.Space(totalSpace);


            EditorGUI.EndProperty();






            if (material == null)
                material = new Material(Shader.Find("Hidden/Internal-Colored"));

            float yMin = (-magnitudeField.floatValue / 2f) + verticalOffsetField.floatValue;
            float yMax = (magnitudeField.floatValue / 2f) + verticalOffsetField.floatValue;

            float width = pos.width;
            float tempVal = 200;

            pos.y += 25;

            EditorGUI.LabelField(new Rect(pos.x, pos.y + tempVal, 50, pos.height), "X Range");
            xMin = EditorGUI.FloatField(new Rect(pos.x + 60, pos.y + tempVal, 70, pos.height), xMin);
            xMax = EditorGUI.FloatField(new Rect(pos.x + 140, pos.y + tempVal, 70, pos.height), xMax);

            
            //pos.width -= 10;

            //EditorGUI.LabelField(new Rect(pos.x - 15, pos.y + 5, pos.width, pos.height),
            //    yMax.ToString());

            //EditorGUI.LabelField(new Rect(pos.x - 15, pos.y + 150 + 25, pos.width, pos.height),
            //yMin.ToString());

            pointCount = EditorGUI.IntField(new Rect(pos.x - 15, pos.y + 150 + 75, pos.width, pos.height),
                new GUIContent("Point Count: "), pointCount);

            float lineCount = 10;


            //EditorGUI.LabelField(new Rect((pos.x - 10), pos.y + 150 + 25, pos.width, pos.height), xMin.ToString());







            GL.PushMatrix();

            GL.Clear(true, false, Color.black);



            GL.Begin(GL.LINES);
            material.SetPass(0);
            GL.Color(Color.black);


            pos.y += 25;
            pos.height = 150;

            DrawOutlineBox(pos, 2);

            //connect points

            //draw 10 lines between the outlines

            DrawGridLines(pos, lineCount);
            PlotGraph((Sigmoid)property.boxedValue, pointCount, pos, xMin, xMax, yMin, yMax);

            GL.End();

            GL.PopMatrix();
            //GUI.EndClip();

            pos.y -= 25;
            pos.height = 30;

            for (int i = 1; i < lineCount + 1; i++)
            {
                EditorGUI.LabelField(new Rect((pos.x - 10) + (((pos.width - 5) / lineCount) * i), pos.y + 150 + 25, pos.width, pos.height),
                    Decimal.Round(new decimal((xMin + ((xMax - xMin) / lineCount * i))), 1).ToString());


            }

            for (int i = 0; i < lineCount / 2f + 1; i++)
            {


                EditorGUI.LabelField(new Rect(pos.x - 20, pos.y + 160 - ((150 / (lineCount / 2f)) * i), pos.width, pos.height),
                    Decimal.Round(new decimal((yMin + ((yMax - yMin) / (lineCount / 2f) * i))), 1).ToString());



            }



            property.serializedObject.ApplyModifiedProperties();



        }


        private void DrawOutlineBox(Rect pos, int pixelWidth)
        {


            for (int i = 0; i < pixelWidth; i++)
            {
                //top line
                GL.Vertex3(pos.x, pos.y + i, 0);
                GL.Vertex3(pos.x + pos.width, pos.y + i, 0);

                //bottom line
                GL.Vertex3(pos.x, pos.y - i + pos.height, 0);
                GL.Vertex3(pos.x + pos.width, pos.y - i + pos.height, 0);


            }

            //Left line
            for (int i = 0; i < pixelWidth; i++)
            {
                //left line
                GL.Vertex3(pos.x + i, pos.y, 0);
                GL.Vertex3(pos.x + i, pos.y + pos.height, 0);

                //right line
                GL.Vertex3(pos.x - i + pos.width, pos.y, 0);
                GL.Vertex3(pos.x - i + pos.width, pos.y + pos.height, 0);


            }

        }


        private void DrawGridLines(Rect pos, float lineCount)
        {
            float x = pos.x + (pos.width / lineCount);

            for (int i = 1; i < lineCount; i++)
            {
                GL.Vertex3(x, pos.y, 0);

                GL.Vertex3(x, pos.y + pos.height, 0);

                x += (pos.width / lineCount);

            }


            float y = pos.y + (pos.height / (lineCount / 2f));

            for (int i = 0; i < (lineCount / 2f); i++)
            {

                GL.Vertex3(pos.x, y, 0);

                GL.Vertex3(pos.x + pos.width, y, 0);

                y += (pos.height / (lineCount / 2f));

            }


        }



        private void PlotGraph(Sigmoid sigmoid, int pointCount, Rect rect, float xMin, float xMax, float yMin, float yMax)
        {

            float plotSpace = rect.xMax - rect.xMin;
            plotSpace /= pointCount;
            //xSpace *= rect.width;

            float geneValueSpace = xMax - xMin;
            geneValueSpace /= pointCount;

            GL.Color(Color.red);
            Vector2 prev = new Vector2(rect.x, rect.y + rect.height);
            Vector2 current = new Vector2();

            for (int i = 0; i < pointCount; i++)
            {
                //get percentage of this between min and max value, then translate that to the rect's
                current.y = sigmoid.GetTraitValue(xMin + (geneValueSpace * i) + sigmoid.horizontalOffset);

                //divide that by maxValue to get percentage closest to one
                current.y /= //yMax;
                (sigmoid.magnitude / 2f);// + sigmoid.verticalOffset);

                current.y *= rect.height / 2f;
                current.y += rect.y - (rect.height * (sigmoid.verticalOffset / (yMax - yMin)));



                //have the y position, need to get x position now

                current.x += plotSpace;
                
                if (i > 0)
                {
                    GL.Vertex(prev);
                    GL.Vertex(current);

                }

                prev = current;

            }

            GL.Color(Color.black);

        }















    }


}