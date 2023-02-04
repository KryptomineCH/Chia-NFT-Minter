using Chia_Metadata;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Minter_UI.CollectionInformation_ns
{ 
    public static partial class CollectionInformation
    {
        /// <summary>
        /// loads attributes and attribute stats into likelyAttributes and allmetadataattributes
        /// </summary>
        private static void GetAttributes(CollectionInformation_Object newInfo)
        {
            if (newInfo == null)
            {
                return;
            }
            if (newInfo.AllMetadataAttributes == null)
            {
                newInfo.AllMetadataAttributes = new ConcurrentDictionary<string, MetadataAttribute>();
            }
                    ConcurrentDictionary<string, int> attributeOcurrences = new ConcurrentDictionary<string, int>();
            foreach (FileInfo file in newInfo.MetadataFiles.Values)
            {
                Metadata data;
                if (!CollectionInformation.GetMetadataFromCache(file, out data))
                {
                    continue;
                }
                // update last known collection metadata
                newInfo.CollectionNumbers.Add((int)data.series_number);
                newInfo.CollectionNumbers = newInfo.CollectionNumbers.OrderBy(x => x).ToList();
                if (newInfo.LastKnownNftMetadata == null || data.series_number > newInfo.LastKnownNftMetadata.series_number)
                {
                    newInfo.LastKnownNftMetadata = data;
                }
                // set attributes
                foreach (MetadataAttribute attr in data.attributes)
                {
                    if (attr == null || newInfo == null)
                    {
                        continue;
                    }
                    if (attr.trait_type == null)
                    {
                        continue;
                    }
                    // add attribute to all attributes dictionary
                    if (!newInfo.AllMetadataAttributes.ContainsKey(attr.trait_type))
                    {
                        newInfo.AllMetadataAttributes[attr.trait_type] = attr;
                    }
                    // increment attribute ocurrences
                    if (!attributeOcurrences.ContainsKey(attr.trait_type))
                    {
                        attributeOcurrences[attr.trait_type] = 1;
                    }
                    else
                    {
                        attributeOcurrences[attr.trait_type]++;
                    }
                    // add attributeValue to attributeValues (used to propose dropdown values)
                    if (attr.value != null)
                    {
                        string? val = attr.value.ToString();
                        if (val != null && newInfo != null && newInfo.AllMetadataAttributeValues != null)
                        {
                            if (!newInfo.AllMetadataAttributeValues.ContainsKey(attr.trait_type))
                            {
                                if (attr != null && newInfo != null && attr.value != null && attr.trait_type != null && newInfo.AllMetadataAttributeValues != null)
                                {
                                    newInfo.AllMetadataAttributeValues[attr.trait_type] = new List<string> { val };
                                }
                            }
                            else if (attr.value != null && !newInfo.AllMetadataAttributeValues[attr.trait_type].Contains(val))
                            {
                                    newInfo.AllMetadataAttributeValues[attr.trait_type].Add(val);
                            }
                        }
                            
                    }
                }
            }
            // set likely attributes list
            List<MetadataAttribute> likelyAttributes = new List<MetadataAttribute>();
            foreach (string key in attributeOcurrences.Keys)
            {
                if (attributeOcurrences[key] > newInfo.MetadataFiles.Count / 2)
                {
                    likelyAttributes.Add(newInfo.AllMetadataAttributes[key]);
                }
            }
            newInfo.LikelyAttributes = likelyAttributes.ToArray();
        }
    }
}
