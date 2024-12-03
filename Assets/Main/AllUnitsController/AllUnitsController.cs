using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    /// <summary>
    /// Unitの設置や除去、Unit同士のアクションを管理する
    public class AllUnitsController : MonoBehaviour
    {
        /// <summary>
        /// PlayerがScene上に設置されている場合このUnitController
        /// </summary>
        public UnitController PlayerUnitController { get; private set; }
        /// <summary>
        /// Scene上に設置されているすべてのEnemyのUnitController
        /// </summary>
        public List<UnitController> EnemyUnitControllers { get; private set; }

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