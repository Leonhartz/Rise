// The Item struct only contains the dynamic item properties and a name, so that
// the static properties can be read from the scriptable object.
//
// Items have to be structs in order to work with SyncLists.
//
// Use .Equals to compare two items. Comparing the name is NOT enough for cases
// where dynamic stats differ. E.g. two pets with different levels shouldn't be
// merged.
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public partial struct Item {
    // hashcode used to reference the real ItemTemplate (can't link to template
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    // dynamic stats (cooldowns etc. later)
    public GameObject petSummoned; // pet that's currently summoned
    public int petHealth; // stored in item while pet unsummoned
    public int petLevel; // stored in item while pet unsummoned
    public long petExperience; // stored in item while pet unsummoned

    // constructors
    public Item(ItemTemplate template) {
        hash = template.name.GetStableHashCode();
        petSummoned = null;
        petHealth = template is PetItemTemplate ? ((PetItemTemplate)template).petPrefab.healthMax : 0;
        petLevel = template is PetItemTemplate ? 1 : 0;
        petExperience = 0;
    }

    // database item property access
    public ItemTemplate template { get { return ItemTemplate.dict[hash]; } }
    public string name { get { return template.name; } }
    public int maxStack { get { return template.maxStack; } }
    public long buyPrice { get { return template.buyPrice; } }
    public long sellPrice { get { return template.sellPrice; } }
    public long itemMallPrice { get { return template.itemMallPrice; } }
    public bool sellable { get { return template.sellable; } }
    public bool tradable { get { return template.tradable; } }
    public bool destroyable { get { return template.destroyable; } }
    public Sprite image { get { return template.image; } }

    // tooltip
    public string ToolTip() {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(template.ToolTip());
        tip.Replace("{PETHEALTH}", petHealth.ToString());
        tip.Replace("{PETLEVEL}", petLevel.ToString());
        tip.Replace("{PETEXPERIENCE}", petExperience.ToString());

        // addon system hooks
        Utils.InvokeMany(typeof(Item), this, "ToolTip_", tip);

        return tip.ToString();
    }
}
