using System;
using System.Collections;
using System.Collections.Generic;
using Units.AI;
using UnityEngine;

namespace Units
{
    /// <summary>
    /// Unit��Prefab�ɃR���|�[�l���g�Ƃ��Ă���Unit�̋������Ǘ�����
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Tooltip("Unit�̎�ނ�ݒ肷��")]
        [SerializeField] UnitType unitType;  

        /// <summary>
        /// ����Unit���G�̏ꍇ�A����AI��ݒ肷��
        /// </summary>
        EnemyAI enemyAI;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    /// <summary>
    /// Unit�̎��
    /// </summary>
    [Serializable] public enum UnitType
    {
        Player,
        Enemy
    }
}
