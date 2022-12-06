using MyGame.UI;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace MyGame.Utils
{

    public sealed class MouseInteractionPresenter : MonoBehaviour
    {
        //[SerializeField] private Camera _camera;

        [Inject] UIFields m_ui;

        [Inject] StateHolder State;

        private void Awake()
        {
            FindSystems();

            /*var nonBlockedByUiFramesStream = Observable.EveryUpdate()
                .Where(_ => !eventSystem.IsPointerOverGameObject());

            var leftClicksStream = nonBlockedByUiFramesStream
                .Where(_ => Input.GetMouseButtonDown(0));
            var rightClicksStream = nonBlockedByUiFramesStream
                .Where(_ => Input.GetMouseButtonDown(1));

            var lmbRays = leftClicksStream
                .Select(_ => _camera.ScreenPointToRay(Input.mousePosition));
            var rmbRays = rightClicksStream
                .Select(_ => _camera.ScreenPointToRay(Input.mousePosition));

            var lmbHitsStream = lmbRays
                .Select(ray => Physics.RaycastAll(ray));
            var rmbHitsStream = rmbRays
                .Select(ray => (ray, Physics.RaycastAll(ray)));

            lmbHitsStream.Subscribe(hits =>
            {
                if (WeHit<ISelectable>(hits, out var selectable))
                {
                    _selectedObjectLeft.SetValue(selectable);
                }
            });*/


            var blockedByUiFramesStream = Observable.EveryUpdate();

            var leftUIStream = blockedByUiFramesStream
                .Where(_ => Input.GetMouseButtonDown(0));

            var rightUIStream = blockedByUiFramesStream
                .Where(_ => Input.GetMouseButtonDown(1));

            var uiRaysLeft = leftUIStream.Select(_ =>
            {
                var pointerEventData = new PointerEventData(m_ui.EventSystem);
                pointerEventData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                m_ui.GraphicRaycaster.Raycast(pointerEventData, results);
                return results;
            });

            var uiRaysRight = rightUIStream.Select(_ =>
            {
                var pointerEventData = new PointerEventData(m_ui.EventSystem);
                pointerEventData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                m_ui.GraphicRaycaster.Raycast(pointerEventData, results);
                return results;
            });

            uiRaysLeft.Subscribe(hits =>
            {
                if (WeHit<ISelectable>(hits, out var selectable))
                {
                    State.SelectedItem.SetValue(selectable);
                }
            });

            uiRaysRight.Subscribe(hits =>
            {
                if (WeHit<ISelectable>(hits, out var selectable))
                {
                    State.ClickedItem.SetValue(selectable);
                }
            });
        }

        private void FindSystems()
        {
            /*if(eventSystem == null)
            {
                eventSystem = MainUIManager.In
            }*/
        }


        private void Init()
        {

            /*rmbHitsStream.Subscribe((ray, hits) =>
            {
                if (WeHit<IAttackable>(hits, out var attackable))
                {
                    _attackablesRMB.SetValue(attackable);
                }
                else if (_groundPlane.Raycast(ray, out var enter))
                {
                    _groundClicksRMB.SetValue(ray.origin + ray.direction * enter);
                }
            });*/
        }

        private bool WeHit<T>(RaycastHit[] hits, out T result) where T : class
        {
            result = default;
            if (hits.Length == 0)
            {
                return false;
            }
            result = hits
                .Select(hit => hit.collider.GetComponentInParent<T>())
                .FirstOrDefault(c => c != null);
            return result != default;
        }

        private bool WeHit<T>(List<RaycastResult> hits, out T result) where T : class
        {
            result = default;
            if (hits.Count == 0)
            {
                return false;
            }
            result = hits
                .Select(hit => hit.gameObject.GetComponentInParent<T>())
                .FirstOrDefault(c => c != null);
            return result != default;
        }
    }
}