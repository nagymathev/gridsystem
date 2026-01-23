# Some sort of Inventory System perhaps with a sprinkle of game.

TODO:
- IMPORTANT:
    - Make Inventory a tool class to preview in editor.
    - While dragging item, there should be an "underlay?" that shows which cells will be occupied when placing.
    - Additionally stop using that mouse location for placing the items because sometimes that is empty or places far away.

- WUDBEGNEISS:

- IDEAS:

Changelog
- While dragging an item show a green or red where the item would be placed, indicating the validity of placement. (2026-01-21)
- (2026-01-17)
    - Make a separate Grid class that defines functions to work with and create grids for use in other contexts.
        - Idea came from reading about delegation in the GoF book.
    - Make a separate InventoryRender class that is purely for visualizing the contents of the inventory.
        - With this all inventories and items should NEVER attempt to render themselves actually. It is the job of this class.
    - Introduce multiple inventories to pass items in between.
    - Inventory class as an object should be placeable anywhere in the UI not influencing functionality.
    - Make the Inventory class have API-like functions for easy interaction.
    - Might need to rewrite how items store their data. Should they store their data in one class, should I have multiple classes for the different states? One thing for sure: **use resources**!
    - Should not be able to interact with the inventory if mouse is outside. Eg.: While dragging mouse goes out of inventory.
    - Separate mouse class that handles interactions with the inventories.
- Create a Grid class that will be used for future grid operations. Stores pointers to objects. (2026-01-15)
- Using textures on top of each other. Allows for defined item textures. (2026-01-11)
- Experiments with using resources for static data. (2026-01-11)
- Multi cell items can block themselves from placement. They should ignore themselves when checking placement. (2026-01-08)
- Translated the code to C# (2026-01-06)
- Don't remove item until it is placed. With this can put item back when trying to place it at an illegal location. (2026-01-01)
- Have items be able to define their own colour. (2025-12-31)
- Make item visible while moving it. (2025-12-31)
- Can no longer place items out of bounds and crash the game (2025-12-31)
- Can move items in the inventory grid (2025-12-30)