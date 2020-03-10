using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distech.CloudRelay.Common.DAL
{
    /// <summary>
    /// Represents the base class for metadata associated to a blob.
    /// </summary>
    public class BlobMetadata
    {
        #region Members

        private readonly Dictionary<string, string> m_Metadata = new Dictionary<string, string>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the original file name of the blob as provided by the user.
        /// </summary>
        public string OriginalFileName
        {
            get { return m_Metadata.TryGetValue(nameof(OriginalFileName), out string filename) ? filename : default(string); }
            set { m_Metadata[nameof(OriginalFileName)] = value; }
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a new <see cref="BlobMetadata"/> instance from a <see cref="IDictionary{string, string}"/> of metadata.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        internal static BlobMetadata FromDictionary(IDictionary<string, string> metadata, bool isBase64Encoded = true)
        {
            BlobMetadata result = new BlobMetadata();

            Type t = typeof(BlobMetadata);
            foreach (var kvp in metadata)
            {
                if (t.GetProperty(kvp.Key) == null)
                {
                    continue;
                }

                string value = kvp.Value;
                if (isBase64Encoded)
                {
                    //all metadata in storage are currently base64 encoded to bypass ASCII only limitation https://github.com/Azure/azure-sdk-for-net/issues/178
                    value = Encoding.UTF8.GetString(Convert.FromBase64String(kvp.Value));
                }

                result.m_Metadata[kvp.Key] = value;
            }

            return result;
        }

        #endregion

        #region Metadata copy

        /// <summary>
        /// Applies the metadata information to the specified azure storage blob.
        /// </summary>
        /// <param name="blob"></param>
        public void ApplyTo(CloudBlob blob)
        {
            foreach (var item in m_Metadata)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    //metadata support only ASCII characters https://github.com/Azure/azure-sdk-for-net/issues/178
                    string base64Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Value));
                    blob.Metadata[item.Key] = base64Value;
                }
                else
                {
                    blob.Metadata.Remove(item.Key);
                }
            }
        }

        #endregion
    }
}
