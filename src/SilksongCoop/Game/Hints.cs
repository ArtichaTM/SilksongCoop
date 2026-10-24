using System.Collections.Generic;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>Short on-screen messages (e.g. D11's "partner too far") and a one-line status.</summary>
internal sealed class Hints
{
    private readonly List<(string Text, float Until)> _messages = [];
    private GUIStyle? _style;

    public string Status { get; set; } = "";

    /// <summary>A standing line at the bottom centre (the downed player's camera key).</summary>
    public string Footer { get; set; } = "";

    public void Show(string text, float seconds = 3f)
    {
        _messages.RemoveAll(message => message.Text == text);
        _messages.Add((text, Time.unscaledTime + seconds));
    }

    public void Draw()
    {
        _messages.RemoveAll(message => message.Until < Time.unscaledTime);
        _style ??= new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        float y = Screen.height * 0.18f;
        foreach ((string text, float _) in _messages)
        {
            GUI.Box(new Rect((Screen.width - 520) / 2f, y, 520, 32), text, _style);
            y += 36;
        }
        if (Footer.Length > 0)
        {
            GUI.Box(new Rect((Screen.width - 520) / 2f, Screen.height - 80, 520, 32), Footer, _style);
        }
        if (Status.Length > 0)
        {
            GUI.Box(new Rect(10, Screen.height - 40, 420, 30), Status, _style);
        }
    }
}
