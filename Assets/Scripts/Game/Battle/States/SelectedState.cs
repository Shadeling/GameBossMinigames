using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame {

    public class SelectedState : IState
    {
        private readonly StateMachine stateMachine;
        private readonly BattleCell cell;

        public SelectedState(StateMachine machine, BattleCell cell)
        {
            this.stateMachine = machine;
            this.cell = cell;
        }


        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        void IState.Update()
        {
            throw new System.NotImplementedException();
        }
    }

}