using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.UI;
using XRL.UI;
using XRL.UI.Framework;
using UnityEngine;


namespace XRL.World.Parts
{
    [Qud.UI.InventoryUIPlugin]
    public class CattlesquatWeightSorter : Qud.UI.IInventoryUIPlugin
    {
        public static readonly string MOD_ID = "WeightSorter";

        private int MySort = -1;
        private InventoryAndEquipmentStatusScreen TheScreen;
        private int LastOtherSort = InventoryAndEquipmentStatusScreen.SORTMODE_CATEGORY;
        
        public MenuOption CMD_TOGGLE = new MenuOption
        {
            Id = "WeightSorterModCmdToggleSort",
            InputCommand = "WeightSorterModCmdToggleSort",
            Description = "Toggle Sort" 
        };
        
        public override void Init()
        {
            // Register a new sort mode for Inventory
            MySort = InventoryAndEquipmentStatusScreen.RegisterSortMode("Weight");
        }
        
        public override bool SortMode(InventoryAndEquipmentStatusScreen Screen, int SortModeID, List<InventoryLineData> ListItems, IEnumerable<InventoryLineData> SearchResult)
        {
            // Sort-by-weight execution for inventory screen 
            if (SortModeID != MySort) return true;
            ListItems.AddRange(SearchResult);
            ListItems.Sort((a, b) => b.go.Weight.CompareTo(a.go.Weight));
            return false;
        }
        
        /// <summary>
        /// Called before each time the not-yet-visible Inventory & Equipment screen about to be displayed. Among other things this is a chance to add commands/handlers for things to appear on bottom line.
        /// example usage to add commands that appear on the bottom line...
        /// Context.menuOptionDescriptions.Add(CMD_OPTIONS);
        /// Context.commandHandlers["CmdOptions"] = MyHandleShowOptions;
        /// </summary>
        public override void BeforeShow(InventoryAndEquipmentStatusScreen Screen, NavigationContext Context)
        {
            TheScreen = Screen;
            Context.menuOptionDescriptions.Add(CMD_TOGGLE);
            Context.commandHandlers["WeightSorterModCmdToggleSort"] = HandleToggle;
            //Context.commandHandlers["CmdOptions"] = HandleToggle;
        }

        /// <summary>
        /// One keystroke toggle between sort by weight and whatever normal sort the player likes 
        /// </summary>
        public void HandleToggle()
        {
            if (TheScreen.CurrentSortMode == MySort)
            {
                TheScreen.CurrentSortMode = LastOtherSort;
            }
            else
            {
                LastOtherSort = TheScreen.CurrentSortMode;
                TheScreen.CurrentSortMode = MySort;
            }

            TheScreen.UpdateViewFromData();
        }

    }
}


[Qud.UI.TradeUIPlugin]
public class CattlesquatWeightSorterTrade : Qud.UI.ITradeUIPlugin
{
    public static readonly string MOD_ID = "WeightSorter";
    public static int WeightSort = -1;
    public static int PriceSort = -1;
    
    private int LastOtherSort = TradeScreen.SORTMODE_CATEGORY;
    public MenuOption CMD_TOGGLE = new MenuOption
    {
        Id = "WeightSorterModCmdToggleSort",
        InputCommand = "WeightSorterModCmdToggleSort",
        Description = "Toggle Sort" 
    };
    public static TradeScreen TradeScreen;
        
    public override void Init()
    {
        // Register two new sorts for the Trade Screen
        PriceSort = TradeScreen.RegisterSortMode("by price");
        WeightSort = TradeScreen.RegisterSortMode("by weight");
    }

    public void HandleToggle()
    {
        if (TradeScreen.CurrentSortMode == WeightSort)
        {
            TradeScreen.CurrentSortMode = LastOtherSort;
        }
        else
        {
            LastOtherSort = TradeScreen.CurrentSortMode;
            TradeScreen.CurrentSortMode = WeightSort;
        }

        TradeScreen.BeforeShow();
        TradeScreen.UpdateViewFromData();
    }

    public override void BeforeShow(TradeScreen Screen, NavigationContext Context)
    {
        TradeScreen ??= Screen;
        
        //Context.menuOptionDescriptions.Add(CMD_TOGGLE); 
        Context.commandHandlers[CMD_TOGGLE.InputCommand] = HandleToggle;
    }
    
    public override bool SortMode(TradeScreen Screen, int SortModeID, int Side, List<TradeLineData> ListItems, List<TradeEntry> FilteredEntries)
    {
        // Sort mode execution for Trade Screen
        
        if (SortModeID == WeightSort)
        {
            FilteredEntries.Sort((a, b) => (b.GO?.Weight ?? 0).CompareTo(a.GO?.Weight ?? 0));
        }
        else if (SortModeID == PriceSort)
        {
            FilteredEntries.Sort((a, b) => (b.GO?.ValueEach ?? 0).CompareTo(a.GO?.ValueEach ?? 0));
        }
        else
        {
            return true;
        }
        
        foreach (var item in FilteredEntries.Where(i => i.GO != null))
        {
            ListItems.Add(TradeLineData.next().set(
                type: TradeLineDataType.Item,
                go: item.GO,
                entry: item,
                numberSelected: Screen.howManySelected(item.GO),
                traderInventory: Side == 0,
                category: null,
                style: TradeLineDataStyle.Interact,
                indent: true
            ));
        }
        
        return false;
    }


    [Qud.UI.TradeLineUIPlugin]
    public class CattlesquatWeightSorterTradeLine : Qud.UI.ITradeLineUIPlugin
    {
        /// <summary>
        /// Called whenever the data for the trade line changes. A chance to modify what is displayed.
        /// </summary>
        public override void SetData(TradeLine Line, TradeLineData Data)
        {
            if (Data is TradeLineData d)
            {
                if (d.type != TradeLineDataType.Category)
                {
                    if (TradeScreen.CostMultiple != 0) // Only when we're seeing prices by default anyway
                    {
                        if (CattlesquatWeightSorterTrade.TradeScreen.CurrentSortMode == CattlesquatWeightSorterTrade.WeightSort)
                        {
                            // During weight sort, also show the weight
                            Line.rightFloatText.SetText(d.go.Weight + "# " + Line.rightFloatText.text);
                        }   
                    }
                }
            } 
        }
    }
}

