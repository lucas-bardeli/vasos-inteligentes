using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace VasosInteligentes.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }
    [Required]
    public string? Nome { get; set; }
    public string? UserName { get; set; }
    [Required]
    public string? Telefone { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string? Email { get; set; }
    [Required]
    public string? Senha { get; set; }
}
