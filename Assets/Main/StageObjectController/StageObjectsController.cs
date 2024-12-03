using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageObjects
{
    /// <summary>
    /// ステージとして読み込まれるPrefabの最も上流の親オブジェクトにアタッチされるコンポーネント   
    /// 主にステージ固有の情報をPrefabのInspector上で設定するためのもの
    /// </summary>
    public class StageObjectsController : MonoBehaviour
    {
        [Tooltip("ステージのチェックポイントObjectを設定する")]
        [SerializeField] List<CheckPoint> checkPoints = new List<CheckPoint>();


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}