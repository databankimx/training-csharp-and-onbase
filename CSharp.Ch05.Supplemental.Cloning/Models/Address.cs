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

namespace CSharp.Ch05.Supplemental.Cloning.Models
{
    /// <summary>
    /// A reference-type child object used to demonstrate whether a clone
    /// shares nested objects with its source or owns independent copies.
    /// </summary>
    internal sealed class Address
    {
        #region Properties
        /// <summary>
        /// Gets or sets the street portion of the address.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a new Address containing the same values as this instance.
        /// </summary>
        public Address DeepClone()
        {
            return new Address
            {
                Street = Street,
                City = City,
                State = State
            };
        }

        /// <summary>
        /// Returns a string representation of the Address object.
        /// </summary>
        /// <returns>A string in the format "Street, City, State".</returns>
        public override string ToString()
        {
            return $"{Street}, {City}, {State}";
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
