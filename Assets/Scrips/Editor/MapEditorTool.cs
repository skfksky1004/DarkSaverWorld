using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class MapEditorTool : EditorWindow {

    private FieldPlane m_planeCreate = null;

    [SerializeField]
    public static string m_editorName { get; set; }

    private int m_widthCell = 0;
    private int m_heightCell = 0;

    private Sprite m_cellSprite = null;
    private GameObject m_baseCell = null;

    /// <summary>
    /// 활성화시 기본값 셋팅
    /// </summary>
    private void OnEnable()
    {
        m_planeCreate = FindObjectOfType<FieldPlane>();
    }

    [MenuItem("Tools/MapEditorTool")]
    public static void ShowEditorTool()
    {
        GetWindow(typeof(MapEditorTool));
    }

    private void OnGUI()
    {
        m_widthCell = EditorGUILayout.IntField("WidthCount",m_widthCell);
        m_heightCell = EditorGUILayout.IntField("HeightCount", m_heightCell);
        
        m_cellSprite = EditorGUILayout.ObjectField("Select Sprite", m_cellSprite, typeof(Sprite),true) as Sprite;

        //  입력받은 값을 기준으로 
        //  플랜을 생성합니다.
        if (GUILayout.Button("Create"))
        {
            CreateFieldPlane();
        }

        //  만들어진 플랜을 삭제합니다.
        if(GUILayout.Button("Delete"))
        {
            DeleteFieldPlane();
        }
    }

    private void CreateFieldPlane()
    {
        if (m_widthCell == 0 || m_heightCell == 0)
        {
            Debug.LogError("Cell count can not be zero.");
            return;
        }

        //선택된 스프라이트가 없을 경우
        if (m_cellSprite == null)
        {
            Debug.LogError("Not Selected SpriteCell.");
            return;
        }

        if (m_planeCreate == null)
        {
            return;
        }

        //각셀 오브젝트 생성
        if (m_cellSprite != null)
        {
            m_baseCell = Resources.Load("Prefabs/BaseCell") as GameObject;
            m_baseCell.GetComponent<SpriteRenderer>().sprite = m_cellSprite;
        }

        //필드 셋팅
        SgtPlaneManager.Instance.Cell_Width = m_planeCreate.m_cellCount_Width = m_widthCell;
        SgtPlaneManager.Instance.Cell_Height = m_planeCreate.m_cellCount_Height = m_heightCell;
        m_planeCreate.cellObject = m_baseCell;
        m_planeCreate.CreatePlane();
    }

    private void DeleteFieldPlane()
    {
        if (m_planeCreate == null)
        {
            return;
        }

        m_planeCreate.ClearPlane();
    }


}
