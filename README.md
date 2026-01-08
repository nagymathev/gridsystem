# Some sort of Inventory System perhaps with a sprinkle of game.

TODO:
- Might need to rewrite how items store their location, or have them not at all store their location.
- Make the Inventory class have API-like functions for easy interaction.
- While dragging an item show a green or red where the item would be placed, indicating the validity of placement.
- Should not be able to interact with the inventory if mouse is outside.
- Figure out some sort of overlay method instead of changing the colors of the items.
    - This could be made using a separate mouse class that handles interactions with the inventories.
- Introduce multiple inventories to pass items in between.

Changelog
- Multi cell items can block themselves from placement. They should ignore themselves when checking placement. (2026-01-08)
- Translated the code to C# (2026-01-06)
- Don't remove item until it is placed. With this can put item back when trying to place it at an illegal location. (2026-01-01)
- Have items be able to define their own colour. (2025-12-31)
- Make item visible while moving it. (2025-12-31)
- Can no longer place items out of bounds and crash the game (2025-12-31)
- Can move items in the inventory grid (2025-12-30)