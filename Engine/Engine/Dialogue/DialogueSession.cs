using System;
using System.Collections.Generic;

namespace Engine.Dialogue;

public sealed class DialogueSession
{
    private readonly string _defaultSpeakerName;
    private readonly IReadOnlyDictionary<string, DialogueNode> _nodes;
    private DialogueNode? _currentNode;
    private DialoguePage? _cachedPage;
    private string _cachedNodeId = string.Empty;

    public DialogueSession(string speakerName, string startNodeId, IReadOnlyDictionary<string, DialogueNode> nodes)
    {
        _defaultSpeakerName = speakerName;
        SpeakerName = speakerName;
        _nodes = nodes;
        _currentNode = ResolveNode(startNodeId);
    }

    public string SpeakerName { get; private set; }

    public bool IsActive => _currentNode is not null;

    public bool TryGetCurrentPage(DialogueContext context, out DialoguePage? page)
    {
        if (!EnsureCurrentNode(context))
        {
            page = null;
            return false;
        }

        if (_cachedPage is not null &&
            string.Equals(_cachedNodeId, _currentNode!.NodeId, StringComparison.Ordinal))
        {
            SpeakerName = _cachedPage.SpeakerName;
            page = _cachedPage;
            return true;
        }

        IReadOnlyList<DialogueResponse> visibleResponses = GetVisibleResponses(context);
        SpeakerName = ResolveSpeakerName(_currentNode!);
        _cachedNodeId = _currentNode!.NodeId;
        _cachedPage = new DialoguePage(SpeakerName, _currentNode.Text, visibleResponses);
        page = _cachedPage;
        return true;
    }

    public DialogueStepResult Advance(DialogueContext context)
    {
        if (!EnsureCurrentNode(context))
            return ClosedResult();

        if (GetVisibleResponses(context).Count > 0)
            return OpenResult();

        if (_currentNode!.CloseAfter || string.IsNullOrWhiteSpace(_currentNode.NextNodeId))
        {
            SetCurrentNode(null);
            return ClosedResult();
        }

        SetCurrentNode(ResolveNode(_currentNode.NextNodeId));
        return EnsureCurrentNode(context) ? OpenResult() : ClosedResult();
    }

    public DialogueStepResult ChooseResponse(string responseId, DialogueContext context)
    {
        if (!EnsureCurrentNode(context))
            return ClosedResult();

        DialogueResponse? selectedResponse = null;
        foreach (DialogueResponse response in GetVisibleResponses(context))
        {
            if (!string.Equals(response.ResponseId, responseId, StringComparison.Ordinal))
                continue;

            selectedResponse = response;
            break;
        }

        if (selectedResponse is null)
            return OpenResult();

        IReadOnlyList<DialogueEffect> effects = selectedResponse.Effects ?? Array.Empty<DialogueEffect>();
        if (selectedResponse.CloseDialogue || string.IsNullOrWhiteSpace(selectedResponse.NextNodeId))
        {
            SetCurrentNode(null);
            return new DialogueStepResult(true, effects);
        }

        SetCurrentNode(ResolveNode(selectedResponse.NextNodeId));
        return !EnsureCurrentNode(context)
            ? new DialogueStepResult(true, effects)
            : new DialogueStepResult(false, effects);
    }

    private bool EnsureCurrentNode(DialogueContext context)
    {
        int guard = 0;

        while (_currentNode is not null)
        {
            if (DialogueConditionEvaluator.Matches(_currentNode.Condition, context))
                return true;

            if (_currentNode.CloseAfter || string.IsNullOrWhiteSpace(_currentNode.NextNodeId))
            {
                SetCurrentNode(null);
                return false;
            }

            SetCurrentNode(ResolveNode(_currentNode.NextNodeId));
            guard++;
            if (guard > _nodes.Count)
            {
                SetCurrentNode(null);
                return false;
            }
        }

        return false;
    }

    private IReadOnlyList<DialogueResponse> GetVisibleResponses(DialogueContext context)
    {
        if (_currentNode is null || _currentNode.Responses.Count == 0)
            return Array.Empty<DialogueResponse>();

        var visibleResponses = new List<DialogueResponse>();
        foreach (DialogueResponse response in _currentNode.Responses)
        {
            if (DialogueConditionEvaluator.Matches(response.Condition, context))
                visibleResponses.Add(response);
        }

        return visibleResponses.Count == 0 ? Array.Empty<DialogueResponse>() : visibleResponses;
    }

    private string ResolveSpeakerName(DialogueNode node)
    {
        return string.IsNullOrWhiteSpace(node.SpeakerName) ? _defaultSpeakerName : node.SpeakerName;
    }

    private DialogueNode? ResolveNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        return _nodes.TryGetValue(nodeId, out DialogueNode? node) ? node : null;
    }

    private void SetCurrentNode(DialogueNode? node)
    {
        _currentNode = node;
        _cachedPage = null;
        _cachedNodeId = string.Empty;
    }

    private static DialogueStepResult ClosedResult()
    {
        return new DialogueStepResult(true, Array.Empty<DialogueEffect>());
    }

    private static DialogueStepResult OpenResult()
    {
        return new DialogueStepResult(false, Array.Empty<DialogueEffect>());
    }
}
