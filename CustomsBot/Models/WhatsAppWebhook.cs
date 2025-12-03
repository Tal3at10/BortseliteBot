using System.Text.Json.Serialization;

namespace CustomsBot.Models
{
    public class WhatsAppWebhook
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<Entry> Entry { get; set; } = new();
    }

    public class Entry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public List<Change> Changes { get; set; } = new();
    }

    public class Change
    {
        [JsonPropertyName("value")]
        public Value Value { get; set; } = new();

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }

    public class Value
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; } = new();

        [JsonPropertyName("contacts")]
        public List<Contact> Contacts { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = new();
    }

    public class Metadata
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    public class Contact
    {
        [JsonPropertyName("profile")]
        public Profile Profile { get; set; } = new();

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    public class Profile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class Message
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public TextMessage? Text { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class TextMessage
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}
