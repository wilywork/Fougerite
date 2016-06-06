using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace Fougerite.Events
{
    public class LootStartEvent
    {
        private bool _cancel;
        private readonly Fougerite.Player _player;
        private readonly LootableObject _lo;
        private readonly Useable _ue;
        private readonly Entity _entity;
        private readonly bool _isobject;
        private readonly uLink.NetworkPlayer _np;
        private Timer _delayer = null;
        private Vector3 _originaloc;

        public LootStartEvent(LootableObject lo, Fougerite.Player player, Useable use, uLink.NetworkPlayer nplayer)
        {
            _lo = lo;
            _ue = use;
            _player = player;
            _np = nplayer;
            foreach (Collider collider in Physics.OverlapSphere(lo._inventory.transform.position, 1.2f))
            {
                if (collider.GetComponent<DeployableObject>() != null)
                {
                    _entity = new Entity(collider.GetComponent<DeployableObject>());
                    _isobject = true;
                    break;
                }
                if (collider.GetComponent<LootableObject>() != null)
                {
                    _entity = new Entity(collider.GetComponent<LootableObject>());
                    _isobject = false;
                    break;
                }
            }
        }

        internal void LootCancelTimer(object sender, ElapsedEventArgs e)
        {
            _delayer.Dispose();
            _lo.transform.position = _originaloc;
        }

        public void Cancel()
        {
            if (!_cancel)
            {
                _cancel = true;
                if (_delayer != null) return;
                /*_lo._useable = _ue;
                _lo._currentlyUsingPlayer = _np;
                _lo._inventory.AddNetListener(_lo._currentlyUsingPlayer);
                _lo.SendCurrentLooter();
                _lo.CancelInvokes();
                _lo.InvokeRepeating("RadialCheck", 0f, 10f);*/
                _originaloc = _lo.transform.position;
                _lo.transform.position = new Vector3(_originaloc.x + 5.5f, _originaloc.y, _originaloc.z + 5.5f);
                // Timer is required, Useable.Eject doesn't allow us to work properly so we will hax with locations
                _delayer = new Timer(100);
                _delayer.Elapsed += LootCancelTimer;
                _delayer.Start();
            }
        }

        public bool IsObject
        {
            get
            {
                return _isobject;
            }
        }

        public Entity Entity
        {
            get
            {
                return _entity;
            }
        }

        public Fougerite.Player Player
        {
            get
            {
                return _player;
            }
        }

        public Useable Useable
        {
            get
            {
                return _ue;
            }
        }

        public LootableObject LootableObject
        {
            get
            {
                return _lo;
            }
        }

        public bool IsCancelled
        {
            get
            {
                return _cancel;
            }
        }

        public string LootName
        {
            get
            {
                return _lo.name;
            }
        }

        public Inventory RustInventory
        {
            get
            {
                return _lo._inventory;
            }
        }

        public string OccupiedText
        {
            get
            {
                return _lo.occupierText;
            }
            set
            {
                _lo.occupierText = value;
            }
        }
    }
}
