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
using System.Configuration;
#endregion

namespace CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration
{
    /// <summary>
    /// Defines a collection (list) of configurable KeywordTypeElement objects
    /// </summary>
    public class KeywordTypeCollection : ConfigurationElementCollection
    {
        #region Required Implementations for ConfigurationElementCollection
        /// <summary>
        /// Method to create a new element in the collection
        /// </summary>
        /// <returns>Keyword Type as ConfigurationElement</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            // This override returns the new element as your custom class
            return new KeywordTypeElement();
        }

        /// <summary>
        /// Obtains the key (identifier) for the passed keyword type element
        /// </summary>
        /// <param name="element">Keyword type element</param>
        /// <returns>Element's "Name" property</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            // This should return the property of the item that you use as its identifier
            return ((KeywordTypeElement)element).Name;
        }
        #endregion

        #region Additional Useful Collection Functionality
        /// <summary>
        /// Allows accessing the collection by numerical indexer
        /// </summary>
        /// <param name="index">Zero-based index position within collection</param>
        /// <returns>Keyword type element at specified position</returns>
        public KeywordTypeElement this[int index]
        {
            get => (KeywordTypeElement)BaseGet(index);
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Allows accessing the collection by identifier (Name)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new KeywordTypeElement this[string name] => (KeywordTypeElement)BaseGet(name);

        /// <summary>
        /// Obtains the index in the collection (if any) of a document type element
        /// </summary>
        /// <param name="element">Keyword type element</param>
        /// <returns>Position index</returns>
        public int IndexOf(KeywordTypeElement element)
        {
            return BaseIndexOf(element);
        }

        /// <summary>
        /// Adds an element to the collection
        /// </summary>
        /// <param name="element">Keyword type element</param>
        public void Add(KeywordTypeElement element)
        {
            // Calls internal method (below)
            BaseAdd(element);
        }

        /// <summary>
        /// Internal add element method (does not throw an exception if the element already exists)
        /// </summary>
        /// <param name="element"></param>
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        /// <summary>
        /// Remove the passed element from the collection if it exists
        /// </summary>
        /// <param name="element">Keyword type element</param>
        public void Remove(KeywordTypeElement element)
        {
            if (BaseIndexOf(element) >= 0)
                BaseRemove(element.Name);
        }

        /// <summary>
        /// Removes the element at the specified index from the collection
        /// </summary>
        /// <param name="index">Position index</param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        /// Removes the element with the specified Name property from the collection
        /// </summary>
        /// <param name="name">Keyword type Name property</param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        /// <summary>
        /// Removes all elements from the collection
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// In order to allow the element to be modified at runtime, we need IsReadOnly to return false
        /// </summary>
        /// <returns>Always false</returns>
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
