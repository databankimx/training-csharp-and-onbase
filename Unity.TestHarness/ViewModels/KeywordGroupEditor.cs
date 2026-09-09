#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hyland.Unity;
using Unity.TestHarness.Models;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: enforces the actual OnBase rule at the UI level, not just at
     * submit time: a SingleInstance group is permanently capped at exactly one
     * GroupInstance (Add Instance disabled, Remove Instance disabled, both from the very
     * first instance), a MultiInstance group can add freely and remove down to (but not
     * below) one. Starts with exactly one instance either way, since both kinds need at
     * least one filled-in set of fields to be worth showing at all.
     */
    #endregion

    /// <summary>
    /// Manages the editable <see cref="GroupInstance"/>(s) for one Keyword Group Type:
    /// exactly one for a SingleInstance group, one or more for a MultiInstance group.
    /// </summary>
    public class KeywordGroupEditor : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The Keyword Group Type being edited.
        /// </summary>
        public KeywordRecordType GroupType { get; }

        /// <summary>
        /// The group's name.
        /// </summary>
        public string Name => GroupType.Name;

        /// <summary>
        /// Whether this group allows more than one instance.
        /// </summary>
        public bool IsMultiInstance => GroupType.RecordType == RecordType.MultiInstance;

        /// <summary>
        /// This group's current instances.
        /// </summary>
        public ObservableCollection<GroupInstance> Instances { get; } = [];
        #endregion

        #region Commands
        /// <summary>
        /// Adds a new, empty instance. Disabled entirely for a SingleInstance group once
        /// its one instance already exists.
        /// </summary>
        public RelayCommand AddInstanceCommand { get; }

        /// <summary>
        /// Removes the <see cref="GroupInstance"/> passed as the command parameter.
        /// Disabled for a SingleInstance group, and once a MultiInstance group is down to
        /// its last remaining instance.
        /// </summary>
        public RelayCommand RemoveInstanceCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the KeywordGroupEditor class
        /// </summary>
        /// <param name="groupType">The Keyword Group Type to edit.</param>
        public KeywordGroupEditor(KeywordRecordType groupType)
        {
            GroupType = groupType;

            AddInstanceCommand = new RelayCommand(_ => AddInstance(), _ => IsMultiInstance);
            RemoveInstanceCommand = new RelayCommand(param => RemoveInstance(param as GroupInstance), _ => IsMultiInstance && Instances.Count > 1);

            AddInstance();
        }

        /// <summary>
        /// Create a new instance of the KeywordGroupEditor class, pre-populated with an
        /// existing document's instance(s) of this group.
        /// </summary>
        /// <param name="groupType">The Keyword Group Type to edit.</param>
        /// <param name="existingInstances">Each existing instance's Keyword Type ID/value pairs.</param>
        public KeywordGroupEditor(KeywordRecordType groupType, IEnumerable<IDictionary<long, string>> existingInstances)
        {
            GroupType = groupType;

            AddInstanceCommand = new RelayCommand(_ => AddInstance(), _ => IsMultiInstance);
            RemoveInstanceCommand = new RelayCommand(param => RemoveInstance(param as GroupInstance), _ => IsMultiInstance && Instances.Count > 1);

            foreach (var existingValues in existingInstances) Instances.Add(new GroupInstance(GroupType, existingValues));
            if (Instances.Count == 0) AddInstance();
        }
        #endregion

        #region Private Methods
        // Add a new, empty instance
        private void AddInstance() => Instances.Add(new GroupInstance(GroupType));

        // Remove the given instance
        private void RemoveInstance(GroupInstance instance)
        {
            if (instance != null) Instances.Remove(instance);
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
