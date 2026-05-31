using HomeCareManager.Core.Configuration;
using HomeCareManager.Core.Services;

int failures = 0;

CheckPasswordHashing();
CheckConfigurationEnvironmentOverride();

if (failures > 0)
{
    Console.Error.WriteLine($"{failures} test(s) failed.");
    return 1;
}

Console.WriteLine("All core smoke tests passed.");
return 0;

void CheckPasswordHashing()
{
    string hash = PasswordHasher.HashPassword("Test1234!");

    Check(
        PasswordHasher.VerifyPassword("Test1234!", hash),
        "PasswordHasher verifies the original password.");

    Check(
        !PasswordHasher.VerifyPassword("WrongPassword", hash),
        "PasswordHasher rejects an incorrect password.");
}

void CheckConfigurationEnvironmentOverride()
{
    const string expected = "datasource=test-host;port=3306;username=test;password=test;database=testdb";
    string? previousValue = Environment.GetEnvironmentVariable("HOMECAREMANAGER_CONNECTION_STRING");

    try
    {
        Environment.SetEnvironmentVariable("HOMECAREMANAGER_CONNECTION_STRING", expected);

        Check(
            DatabaseConfiguration.GetConnectionString() == expected,
            "DatabaseConfiguration uses the environment override.");
    }
    finally
    {
        Environment.SetEnvironmentVariable("HOMECAREMANAGER_CONNECTION_STRING", previousValue);
    }
}

void Check(bool condition, string message)
{
    if (condition)
    {
        Console.WriteLine($"PASS: {message}");
        return;
    }

    failures++;
    Console.Error.WriteLine($"FAIL: {message}");
}
