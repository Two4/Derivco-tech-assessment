#run the setup script to create the DB and the schema in the DB
#do this in a loop because the timing for when the SQL instance is ready is indeterminate
for i in {1..60};
do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Derivco_secure_pwd1!" -d master -i /sql-setup/instnwnd.sql
    if [ $? -eq 0 ]
    then
        echo "/sql-setup/instnwnd.sql completed"
        break
    else
        echo "not ready yet..."
        sleep 1
    fi
done
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Derivco_secure_pwd1!" -d Northwind -i /sql-setup/sp_GetOrderSummary.sql
echo "created stored procedure 'pr_GetOrderSummary'"
echo "DB setup complete"