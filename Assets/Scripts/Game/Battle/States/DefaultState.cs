using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public class DefaultState : IState
    {
        private readonly StateMachine stateMachine;
        private readonly BattleCell cell;

        public DefaultState(StateMachine machine, BattleCell cell)
        {
            this.stateMachine = machine;
            this.cell = cell;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        void IState.Update()
        {
        }
    }

}
