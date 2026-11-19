using Assets.Scripts.Buildings;
using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Character
{
    public class InteractableService : IInteractableService, ITickable, IDisposable
    {
        private readonly List<BaseBuilding> _buildings = new List<BaseBuilding>();
        private readonly IInput _inputReader;
        private readonly Player _player;

        [Inject]
        public InteractableService(IInput input, Player player)
        {
            _inputReader = input;
            _player = player;
            _inputReader.OnIntection += Interact;
        }

        public IInteractable Current { get; private set; }

        public void Set(IInteractable interactable)
        {
            BaseBuilding building = interactable as BaseBuilding;
            if (building != null && _buildings.Contains(building) == false) _buildings.Add(building);
        }

        public void Clear(IInteractable interactable)
        {
            BaseBuilding building = interactable as BaseBuilding;
            if (building != null) _buildings.Remove(building);
            if (Current == interactable)
            {
                building.SetSelected(false);
                Current = null;
            }
        }

        public void Tick()
        {
            _buildings.RemoveAll(building => building == null);
            _buildings.Sort((first, second) =>
            {
                int position = first.transform.position.x.CompareTo(second.transform.position.x);
                return position != 0 ? position : first.GetInstanceID().CompareTo(second.GetInstanceID());
            });
            float[] distances = new float[_buildings.Count];
            bool[] available = new bool[_buildings.Count];
            int current = -1;
            for (int i = 0; i < _buildings.Count; i++)
            {
                distances[i] = Mathf.Abs(_buildings[i].transform.position.x - _player.transform.position.x);
                available[i] = _buildings[i].CanInteract;
                if (Current == _buildings[i]) current = i;
            }

            int selected = BuildingSelection.Select(distances, available, current);
            BaseBuilding next = selected < 0 ? null : _buildings[selected];
            if (Current == next) return;
            if (Current is BaseBuilding previous && previous != null) previous.SetSelected(false);
            Current = next;
            if (next != null) next.SetSelected(true);
        }

        public void Interact()
        {
            Tick();
            Current?.Interactable();
            Tick();
        }

        public void Dispose()
        {
            _inputReader.OnIntection -= Interact;
        }
    }
}
