# Dialogue JSON Guide

Dialogue content for the game is loaded from:

- `Sandbox/Sandbox/Game/Content/Dialogue/**/*.json`

These files are merged together at startup, then validated. If a file has bad IDs, missing links, or invalid effect types, the game fails fast on boot.

## Top-Level Structure

Each file can contain either or both of these arrays:

```json
{
  "conversations": [],
  "triggers": []
}
```

- `conversations`: dialogue trees that NPCs or world trigg
- ers can start.
- `triggers`: one-off world interactions tied to a map/object/position.

## IDs

IDs are string keys. They are how systems connect to each other.

- `conversationId`
  - Unique across all dialogue files.
  - Referenced by NPCs and world triggers.
  - Example: `npc_shop_keeper_conversation`

- `variantId`
  - Unique within a conversation in practice.
  - Used only for organizing/debugging variants.
  - Example: `morning_offer`

- `nodeId`
  - Unique within one variant.
  - Used by `startNodeId`, `nextNodeId`, and response links.
  - Example: `offer`, `accepted`, `declined`

- `responseId`
  - Unique within a single node.
  - Used internally when the player picks a response.
  - Example: `accept`, `decline`, `ask_why`

- `triggerId`
  - Unique across all triggers.
  - Used only for organizing/debugging triggers.
  - Example: `town_intro_trigger`

- Quest IDs
  - Refer to entries in the quest log.
  - Example: `travel_errand_intro`

- Flag IDs
  - Refer to progression flags stored on the player.
  - Example: `doctor_warning_seen`

## Conversation Shape

`conversations` contain one or more `variants`.

- `speakerName`
  - Default speaker for the conversation.
- `variants`
  - Conditional versions of the same conversation.
  - Highest `priority` wins.
  - If multiple top-priority variants match, `weight` is used for random selection.

### Variant Fields

- `variantId`: debug/organization ID.
- `priority`: higher number wins over lower number.
- `weight`: random weight among matching variants of the same priority. Minimum is `1`.
- `speakerName`: optional speaker override for the whole variant.
- `startNodeId`: first node in this variant.
- `conditions`: optional rules that decide whether this variant is active.
- `nodes`: the actual dialogue nodes.

## Nodes

A node is one piece of dialogue text.

- `nodeId`: link target within the variant.
- `speakerName`: optional speaker override for this one node.
- `text`: what is shown in the dialogue box.
- `nextNodeId`: auto-advance target when there are no responses.
- `closeAfter`: if `true`, dialogue closes after this node is shown and advanced.
- `conditions`: optional rules for whether this node is allowed.
- `responses`: player choices.

### Node Flow Rules

- If a node has visible responses, the player must choose one.
- If a node has no responses:
  - `closeAfter: true` closes the dialogue after the player advances.
  - otherwise `nextNodeId` is followed.
- If a node’s `conditions` do not match:
  - the runtime skips to `nextNodeId` if possible.
  - otherwise the conversation closes.

## Responses

Responses are the player’s menu choices.

- `responseId`: internal response key.
- `text`: visible label in the response menu.
- `nextNodeId`: node to move to after choosing it.
- `closeDialogue`: if `true`, the conversation ends immediately after selection.
- `conditions`: optional rules controlling whether the choice appears.
- `effects`: things that happen when chosen.

### Response Flow Rules

- If `closeDialogue` is `true`, the dialogue closes after applying effects.
- If `closeDialogue` is `false` and `nextNodeId` is set, the dialogue moves to that node.
- A response with conditions only appears when those conditions match.

## Effects

Currently supported effect types:

- `accept_quest`
  - Adds a quest to the quest log.
  - `value`: quest ID.
  - `extra`: optional quest title shown in UI. If omitted, `value` is reused.

- `set_flag`
  - Adds a progression flag.
  - `value`: flag ID.

- `add_lore`
  - Adds a lore/journal entry string.
  - `value`: lore text or lore ID, depending on how you choose to use it.

Example:

```json
{
  "effectType": "accept_quest",
  "value": "travel_errand_intro",
  "extra": "Travel Errand"
}
```

## Conditions

Conditions can be placed on:

- variants
- nodes
- responses

All listed conditions must match for that object to be active.

### Time Fields

- `earliestMinutes`
- `latestMinutes`

These are minutes since midnight.

- `0` = `12:00 AM`
- `360` = `6:00 AM`
- `720` = `12:00 PM`
- `1080` = `6:00 PM`

If `earliestMinutes <= latestMinutes`, the range is normal.

- Example: `360` to `719` means morning only.

If `earliestMinutes > latestMinutes`, the range wraps past midnight.

- Example: `1080` to `359` means evening through early morning.

### Calendar Fields

- `minDayNumber`
- `maxDayNumber`
- `allowedWeekdays`
- `allowedSeasons`

`allowedWeekdays` and `allowedSeasons` must match the names configured in calendar settings exactly in spirit. Matching is case-insensitive.

Current defaults:

- Weekdays: `Monday` through `Sunday`
- Seasons: `Spring`, `Summer`, `Autumn`, `Winter`

### Progression Fields

- `requiredFlags`
- `excludedFlags`
- `requiredQuestIds`
- `excludedQuestIds`

Examples:

- `requiredFlags: ["doctor_warning_seen"]`
  - only active after that flag is set.
- `excludedQuestIds: ["travel_errand_intro"]`
  - only active before that quest is accepted.

### Random Field

- `randomChance`
  - Decimal from `0` to `1`.
  - `1` means always eligible.
  - `0.25` means 25% chance when evaluated.

Use this carefully on variants, nodes, or responses intended for random events.

## World Triggers

Triggers start a conversation without needing an NPC.

- `triggerId`: unique trigger ID.
- `priority`: if multiple triggers overlap, higher priority is tried first.
- `mapAssetName`: map where this trigger exists.
- `conversationId`: conversation to start.
- `speakerName`: optional speaker override.
- `triggerObjectName`: optional Tiled object name to use as trigger zone.
- `fallbackX`, `fallbackY`: optional fallback position in world coordinates.
- `interactionRadius`: radius around the fallback position.

Trigger rules:

- If `triggerObjectName` is set and found on the map, that area is used.
- Otherwise the fallback position and radius are used.
- Triggers are started by the same interaction key used for NPCs.

## Validation Rules

The validator currently checks:

- duplicate `conversationId`
- duplicate `triggerId`
- missing `startNodeId`
- duplicate `nodeId` inside a variant
- duplicate `responseId` inside a node
- missing linked node targets
- unknown `effectType`
- NPCs referencing missing conversations
- triggers referencing missing conversations

## Naming Recommendations

- Use lowercase snake-style IDs for conversations, quests, flags, nodes, and responses.
- Prefix by type when useful:
  - `npc_...`
  - `world_...`
  - `event_...`
  - `quest_...`
  - `flag_...`

Examples:

- `npc_shop_keeper_conversation`
- `world_intro_thoughts`
- `doctor_warning_seen`
- `travel_errand_intro`

## Authoring Pattern Recommendations

- One file per NPC or event chain.
- Keep one conversation focused on one interaction purpose.
- Use variants for high-level state splits:
  - time of day
  - weekday
  - season
  - quest stage
  - important flags
- Use nodes for the actual step-by-step flow.
- Use response conditions for optional choices that unlock later.
- Use priorities for deterministic story progression.
- Use weights and `randomChance` for ambient/random events.

## Safe Starting Pattern

1. Start with one conversation and one variant.
2. Add nodes and response links.
3. Add simple quest/flag conditions.
4. Add weekday/season/time conditions after the base flow works.
5. Add randomization only when deterministic behavior is already solid.

See `template.json` in this same folder for a commented example showing every supported field.
