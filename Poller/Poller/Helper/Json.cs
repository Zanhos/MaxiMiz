﻿using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Poller.Helper
{
    public static class Json
    {
        private static readonly JsonSerializer jsonSerializer = new JsonSerializer();

        

        /// <summary>
        /// Deserializes a string into an instance of an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON to.</typeparam>
        /// <param name="json">The string containing the JSON data.</param>
        /// <returns>The Derserialized instance of <see cref="T"/>.</returns>
        public static T Deserialize<T>(string json)
            where T : class
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }

        /// <summary>
        /// Deserializes A stream into an instance of an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON to.</typeparam>
        /// <param name="stream">The stream containing the JSON data.</param>
        /// <returns>The Derserialized instance of <see cref="T"/>.</returns>
        public static T Deserialize<T>(Stream stream)
            where T : class
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }

        /// <summary>
        /// Deserializes A HttpResponseMessage its content into an instance of an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON to.</typeparam>
        /// <param name="stream">The stream containing the JSON data.</param>
        /// <returns>The Derserialized instance of <see cref="T"/>.</returns>
        public static async Task<T> DeserializeAsync<T>(HttpResponseMessage message)
            where T : class
        {
            using (var stream = await message.Content.ReadAsStreamAsync())
            {
                return Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// Convert object to json string.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>JSON representation of the object</returns>
        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj);
    }
}
