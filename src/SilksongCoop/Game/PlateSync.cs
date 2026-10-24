using System;
using System.Collections.Generic;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Pressure plates and the carriage they drive (Bone_11b, Bone_16), 1.5 a: both only feel the local hero.
/// A plate pressed in either game drops in the partner's too, and a carriage button the partner stands on
/// counts as pressed in both games, so each game's carriage gets the same input.
/// </summary>
internal sealed class PlateSync : Module
{
    private const float ReindexCooldown = 1f;

    private readonly Dictionary<int, PressurePlateBase> _plates = [];
    private readonly HashSet<int> _remoteDrops = [];
    private readonly List<Carriage> _carriages = [];
    private float _lastIndex = -ReindexCooldown;

    public PlateSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    public override void Register(Session session)
    {
        session.On(MessageType.PlatePressed, OnPlatePressed);
        session.On(MessageType.LiftButton, OnLiftButton);
    }

    public override void Update()
    {
        if (!Coop.Rooms.PartnerHere)
        {
            _carriages.ForEach(carriage => carriage.ReleasePartner());
        }
    }

    public override void Dispose()
    {
        Coop.Rooms.Entered -= OnEntered;
        Unhook();
    }

    /// <summary>A plate finished dropping here; one the partner pressed is not sent back.</summary>
    public void OnLocalPress(PressurePlateBase plate)
    {
        int id = EnemySync.IdOf(plate.gameObject);
        if (_remoteDrops.Remove(id) || !Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(plate.gameObject.scene))
        {
            return;
        }
        Session?.Send(MessageType.PlatePressed, w => w.Int(Coop.Rooms.CurrentHash).Int(id));
    }

    private void OnPlatePressed(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        int id = reader.Int();
        if (forThisRoom && Find(id) is { } plate && plate.col.enabled)
        {
            _remoteDrops.Add(id);
            plate.StartDrop(force: true);
        }
    }

    private void OnLiftButton(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        int id = reader.Int();
        bool right = reader.Bool();
        bool pressed = reader.Bool();
        Carriage? carriage = _carriages.Find(each => each.Id == id);
        if (forThisRoom && carriage is not null)
        {
            carriage.SetPartner(right, pressed);
        }
    }

    private void OnEntered(string scene)
    {
        _plates.Clear();
        _remoteDrops.Clear();
        _lastIndex = -ReindexCooldown;
        Unhook();
        var lifts = UnityEngine.Object.FindObjectsByType<ManualLift>(FindObjectsSortMode.None);
        foreach (ManualLift lift in lifts)
        {
            if (Coop.Rooms.Holds(lift.gameObject.scene))
            {
                _carriages.Add(new Carriage(lift, SendButton));
            }
        }
    }

    private void SendButton(Carriage carriage, bool right, bool pressed)
    {
        if (Coop.Rooms.PartnerHere)
        {
            int room = Coop.Rooms.CurrentHash;
            int id = carriage.Id;
            Session?.Send(MessageType.LiftButton, w => w.Int(room).Int(id).Bool(right).Bool(pressed));
        }
    }

    private void Unhook()
    {
        _carriages.ForEach(carriage => carriage.Unhook());
        _carriages.Clear();
    }

    private PressurePlateBase? Find(int id)
    {
        if (!_plates.TryGetValue(id, out PressurePlateBase? plate) || !plate)
        {
            Reindex();
            _plates.TryGetValue(id, out plate);
        }
        return plate ? plate : null;
    }

    private void Reindex()
    {
        if (Time.unscaledTime - _lastIndex < ReindexCooldown || Coop.Rooms.Current is null)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        var plates = UnityEngine.Object.FindObjectsByType<PressurePlateBase>(FindObjectsSortMode.None);
        foreach (PressurePlateBase plate in plates)
        {
            if (Coop.Rooms.Holds(plate.gameObject.scene))
            {
                _plates[EnemySync.IdOf(plate.gameObject)] = plate;
            }
        }
    }

    /// <summary>One carriage: its buttons count as pressed while either player stands on them.</summary>
    private sealed class Carriage
    {
        private readonly ManualLift _lift;
        private readonly Action<bool> _onLeft;
        private readonly Action<bool> _onRight;
        private bool _partnerLeft;
        private bool _partnerRight;

        public Carriage(ManualLift lift, Action<Carriage, bool, bool> send)
        {
            _lift = lift;
            Id = EnemySync.IdOf(lift.gameObject);
            // Added after the lift's own handlers, so these run last and see the local press already set.
            _onLeft = pressed =>
            {
                send(this, false, pressed);
                Apply();
            };
            _onRight = pressed =>
            {
                send(this, true, pressed);
                Apply();
            };
            if (lift.buttonLeft)
            {
                lift.buttonLeft.DepressedChange += _onLeft;
            }
            if (lift.buttonRight)
            {
                lift.buttonRight.DepressedChange += _onRight;
            }
        }

        public int Id { get; }

        public void SetPartner(bool right, bool pressed)
        {
            if (right)
            {
                _partnerRight = pressed;
            }
            else
            {
                _partnerLeft = pressed;
            }
            Apply();
        }

        public void ReleasePartner()
        {
            if (_partnerLeft || _partnerRight)
            {
                _partnerLeft = _partnerRight = false;
                Apply();
            }
        }

        public void Unhook()
        {
            if (_lift && _lift.buttonLeft)
            {
                _lift.buttonLeft.DepressedChange -= _onLeft;
            }
            if (_lift && _lift.buttonRight)
            {
                _lift.buttonRight.DepressedChange -= _onRight;
            }
        }

        private void Apply()
        {
            if (!_lift)
            {
                return;
            }
            _lift.isLeftPressed = _partnerLeft || (_lift.buttonLeft && _lift.buttonLeft.IsDepressed);
            _lift.isRightPressed = _partnerRight || (_lift.buttonRight && _lift.buttonRight.IsDepressed);
            _lift.UpdateDirection(overrideCall: true);
        }
    }
}
