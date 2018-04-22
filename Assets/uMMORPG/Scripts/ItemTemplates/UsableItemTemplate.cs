// only usable items need minLevel and usage functions
using System.Text;
using UnityEngine;

public abstract class UsableItemTemplate : ItemTemplate {
    [Header("Usage")]
    public int minLevel; // level required to use the item

    // usage ///////////////////////////////////////////////////////////////////
    public virtual bool CanUse(Player player, int inventoryIndex) {
        return player.level >= minLevel;
    }

    public abstract void Use(Player player, int inventoryIndex);

    // tooltip
    public override string ToolTip() {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{MINLEVEL}", minLevel.ToString());
        return tip.ToString();
    }
}
