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

#region Directives
using System.Configuration;
#endregion

namespace OnBase.Preprocessor.Models.Configuration
{
    /// <summary>
    /// Defines a collection of character replacement elements
    /// </summary>
    public class CharacterCollection : ConfigurationElementCollection
    {
        #region ConfigurationElementCollection Required Methods
        /// <summary>
        /// Create a new CharacterElement instance
        /// </summary>
        /// <returns><see cref="CharacterElement"/></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CharacterElement();
        }

        /// <summary>
        /// Defines the key field for a CharacterElement
        /// </summary>
        /// <param name="element">Character Element</param>
        /// <returns>Original character</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CharacterElement)element).Original;
        }
        #endregion

        #region Additional Collection Methods
        /// <summary>
        /// Allow elements to be retrieved by index
        /// </summary>
        /// <param name="index">Index int</param>
        /// <returns><see cref="CharacterElement"/></returns>
        public CharacterElement this[int index]
        {
            get => (CharacterElement)BaseGet(index);
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Allow elements to be retrieved by key
        /// </summary>
        /// <param name="original">Original character</param>
        /// <returns><see cref="CharacterElement"/></returns>
        // Explicit (object) cast is required here, not decorative: char converts implicitly to
        // int (a standard numeric conversion), which C# prefers over boxing to object during
        // overload resolution. Without the cast, this silently resolves to BaseGet(int index)
        // instead of the intended BaseGet(object key) - treating each character's numeric code
        // point as a collection index rather than a key, and throwing "index out of range" for
        // any character whose code point is >= the collection's actual size.
        public CharacterElement this[char original] => (CharacterElement)BaseGet((object)original);

        /// <summary>
        /// Obtain index of specified element
        /// </summary>
        /// <param name="element"><see cref="CharacterElement"/></param>
        /// <returns>Index</returns>
        public int IndexOf(CharacterElement element)
        {
            return BaseIndexOf(element);
        }

        /// <summary>
        /// Add element to list
        /// </summary>
        /// <param name="element"><see cref="CharacterElement"/></param>
        public void Add(CharacterElement element)
        {
            BaseAdd(element);
        }

        /// <summary>
        /// Do not throw error when writing duplicate element
        /// </summary>
        /// <param name="element"><see cref="CharacterElement"/></param>
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        /// <summary>
        /// Remove specified element
        /// </summary>
        /// <param name="element"><see cref="CharacterElement"/></param>
        public void Remove(CharacterElement element)
        {
            if (BaseIndexOf(element) >= 0) BaseRemove(element.Original);
        }

        /// <summary>
        /// Remove element at specified index
        /// </summary>
        /// <param name="index">Index</param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        /// Remove specified element by key
        /// </summary>
        /// <param name="original">Original character</param>
        public void Remove(char original)
        {
            BaseRemove(original);
        }

        /// <summary>
        /// Remove all elements
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// Allow the class elements to be editable
        /// </summary>
        /// <returns>false (not read-only)</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
