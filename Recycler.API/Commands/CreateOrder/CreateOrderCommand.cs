using System.Text.Json.Serialization;
using MediatR;
using Recycler.API.Converters;
using Recycler.API.Models;

namespace Recycler.API;

public class CreateOrderCommand : IRequest<GenericResponse<OrderDto>>
{
    [JsonConverter(typeof(HyphenToUnderscoreConverter))]
    public required string CompanyName { get; set; }
    public required IEnumerable<CreateOrderItemDto> OrderItems { get; set; }
}