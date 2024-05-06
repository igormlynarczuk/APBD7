namespace WebApplication1.Repositories;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

//PROJEKT MOŻE W PEŁNI NIE DZIAŁAĆ Z POWODU PROBLEMÓW Z RIDEREM NIE JESTEM W STANIE URUCHOMIĆ PROJEKTU ITP

public class ProductWarehouseRepository
{
    private readonly IConfiguration _configuration;

    public ProductWarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int AddProductToWarehouse(int idProduct, int idWarehouse, int amount, DateTime createdAt)
    {
        if (!DoesProductExist(idProduct))
            throw new ArgumentException("Product with the provided ID does not exist.");

        if (!DoesWarehouseExist(idWarehouse))
            throw new ArgumentException("Warehouse with the provided ID does not exist.");

        if (!DoesOrderExist(idProduct, amount, createdAt))
            throw new InvalidOperationException("There is no valid order to fulfill.");

        if (IsOrderFulfilled(idProduct))
            throw new InvalidOperationException("The order has already been fulfilled.");

        decimal price = GetProductPrice(idProduct);

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        SqlTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction;

        try
        {
            // Aktualizacja zamówienia
            string updateOrderQuery = "UPDATE [Order] SET FulfilledAt = @CreatedAt WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
            command.CommandText = updateOrderQuery;
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.ExecuteNonQuery();

            // Wstawienie rekordu do tabeli Product_Warehouse
            string insertProductWarehouseQuery = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
            command.CommandText = insertProductWarehouseQuery;
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdOrder", GetOrderId(idProduct, amount, createdAt));
            command.Parameters.AddWithValue("@Price", price * amount);
            command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        // Zwrócenie identyfikatora nowego rekordu w Product_Warehouse
        string getIdentityQuery = "SELECT SCOPE_IDENTITY()";
        command.CommandText = getIdentityQuery;
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private bool DoesProductExist(int idProduct)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private bool DoesWarehouseExist(int idWarehouse)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private bool DoesOrderExist(int idProduct, int amount, DateTime createdAt)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private bool IsOrderFulfilled(int idProduct)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private decimal GetProductPrice(int idProduct)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    private int GetOrderId(int idProduct, int amount, DateTime createdAt)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
