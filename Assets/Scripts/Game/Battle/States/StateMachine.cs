using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyGame
{

    public class StateMachine
    {
        public event StateHandler OnStateChanged;
        public delegate void StateHandler(IState state, IState oldState);

        private IState currentState;
        private IState oldState;

        public IState CurrentState => currentState;
        public IState OldState => oldState;

        public void ChangeState(IState state)
        {
            oldState = currentState;
            currentState.Exit();

            currentState = state;
            state.Enter();

            OnStateChanged?.Invoke(CurrentState, OldState);
        }

        public void Init(IState state)
        {
            currentState = state;
            state.Enter();
        }

        public void Update()
        {
            currentState.Update();
        }

    }


    public interface IState
    {
        void Enter();

        void Exit();

        void Update();
    }


}