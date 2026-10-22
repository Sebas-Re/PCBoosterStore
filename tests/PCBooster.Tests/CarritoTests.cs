using System.Text.Json;
using PCBooster.Dominio;

namespace PCBooster.Tests;

public class CarritoTests
{
    [Fact]
    public void ArrancaVacio()
    {
        var carrito = new Carrito();

        Assert.True(carrito.Vacio);
        Assert.Equal(0, carrito.CantidadUnidades);
        Assert.Equal(0m, carrito.Total);
    }

    [Fact]
    public void AgregarElMismoArticuloSumaCantidad()
    {
        var carrito = new Carrito();
        carrito.Agregar("gb1", "Gabinete", 3500m, 10);
        carrito.Agregar("GB1", "Gabinete", 3500m, 10);
        carrito.Agregar("fnt1", "Fuente", 7300m, 10);

        Assert.Equal(2, carrito.Items.Count);
        Assert.Equal(3, carrito.CantidadUnidades);
        Assert.Equal(14300m, carrito.Total);
    }

    [Fact]
    public void NoPermiteSuperarElStock()
    {
        var carrito = new Carrito();

        Assert.True(carrito.Agregar("gb8", "Gabinete", 100m, 1));
        Assert.False(carrito.Agregar("gb8", "Gabinete", 100m, 1));
        Assert.Equal(1, carrito.CantidadUnidades);
    }

    [Fact]
    public void QuitarYVaciar()
    {
        var carrito = new Carrito();
        carrito.Agregar("gb1", "Gabinete", 3500m, 10);
        carrito.Agregar("fnt1", "Fuente", 7300m, 10);

        Assert.True(carrito.Quitar("GB1"));
        Assert.False(carrito.Quitar("gb1"));
        Assert.Single(carrito.Items);
        carrito.Vaciar();
        Assert.True(carrito.Vacio);
    }

    [Fact]
    public void SobreviveAlSerializarseEnLaSesion()
    {
        var carrito = new Carrito();
        carrito.Agregar("gb1", "Gabinete", 3500m, 10);
        carrito.Agregar("gb1", "Gabinete", 3500m, 10);

        var copia = JsonSerializer.Deserialize<Carrito>(JsonSerializer.Serialize(carrito))!;

        Assert.Equal(2, copia.CantidadUnidades);
        Assert.Equal(7000m, copia.Total);
    }
}
