using Chia_Metadata;
using System.Collections.Concurrent;

namespace Chia_NFT_Minter.CollectionInformation_ns
{
    public static partial class CollectionInformation
    {
        /// <summary>
        /// loads attributes and attribute stats into likelyAttributes and allmetadataattributes
        /// </summary>
        private static void GetAttributes(CollectionInformation_Object newInfo)
        {
            ConcurrentDictionary<string, int> attributeOcurrences = new ConcurrentDictionary<string, int>();
            foreach (FileInfo fi in newInfo.MetadataFiles.Values)
            {
                Metadata data = IO.Load(fi.FullName);
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
                    if (!newInfo.AllMetadataAttributeValues.ContainsKey(attr.trait_type))
                    {
                        newInfo.AllMetadataAttributeValues[attr.trait_type] = new List<string> { attr.value.ToString() };
                    }
                    else
                    {
                        if (!newInfo.AllMetadataAttributeValues[attr.trait_type].Contains(attr.value.ToString()))
                        {
                            newInfo.AllMetadataAttributeValues[attr.trait_type].Add(attr.value.ToString());
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
