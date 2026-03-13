namespace Sandbox.Game.Scene.Npc;

internal sealed record NpcQuestOffer(
    string QuestId,
    string Title,
    string OfferText,
    string AcceptedText,
    string DeclinedText,
    string AlreadyAcceptedText);
