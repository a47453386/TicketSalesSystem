using Microsoft.Extensions.Options;
using System.Text.Json;

// JsonConvert.DeserializeObject<IEnumerable<PetFoodData>>(response);//將JSON資料反序列化為List<PetFoodData>物件集合應該改成這個
namespace TicketSalesSystem.Helpers
{
    public static class SessionHelper
    {
        //JsonSerializerOptions 用於序列化和反序列化 Session 中的物件，用來控制 JSON 要怎麼轉、怎麼讀
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            //「大小寫不敏感」
            PropertyNameCaseInsensitive = true,

             // 如果你的資料有層級關係，這能避免某些序列化問題
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        //使用定義好的 Options 進行序列化(ISession:，T:泛型 (Generics))
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            
            session.SetString(key, JsonSerializer.Serialize(value, options));
        }

        //將物件轉成 JSON 字串，然後存到 Session 中
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            // 使用定義好的 Options 進行反序列化
            return value == null ? default : JsonSerializer.Deserialize<T>(value, options);
        }

    }
}
