using decofurn_exercise;


static void ExecuteCsvImport()
{
    string csvFilePath = "data.csv";
    string connectionString = "Data Source=ZEE\\SQLEXPRESS01;Initial Catalog=decofurn_ex;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

    try
    {
        Console.WriteLine("=== Starting CSV Import Process ===");
        //Console.WriteLine($"CSV File Path: {csvFilePath}");
        //Console.WriteLine($"Database Connection: {connectionString}");

        CSVDataParser.ImportCsvToDatabase(csvFilePath, connectionString);

    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during the import process: {ex.Message}");
    }
}

ExecuteCsvImport();