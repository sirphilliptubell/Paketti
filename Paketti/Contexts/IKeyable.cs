using System;
using System.Collections.Generic;
using System.Text;

namespace Paketti.Contexts
{
    /// <summary>
    /// Represents an object which can be identified by a key.
    /// </summary>
    public interface IKeyable
    {
        /// <summary>
        /// Gets the key that identifies an object.
        /// </summary>
        /// <value>
        /// The object's key.
        /// </value>
        string Key { get; }
    }
}