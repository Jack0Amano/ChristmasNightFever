using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    /// <summary>
    /// Unit�̐ݒu�⏜���AUnit���m�̃A�N�V�������Ǘ�����
    public class AllUnitsController : MonoBehaviour
    {
        /// <summary>
        /// Player��Scene��ɐݒu����Ă���ꍇ����UnitController
        /// </summary>
        public UnitController PlayerUnitController { get; private set; }
        /// <summary>
        /// Scene��ɐݒu����Ă��邷�ׂĂ�Enemy��UnitController
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