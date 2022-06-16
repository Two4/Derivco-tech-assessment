FROM mcr.microsoft.com/mssql/server:2019-latest
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Derivco_secure_pwd1!
EXPOSE 1433
WORKDIR /sql-setup
ADD https://raw.githubusercontent.com/microsoft/sql-server-samples/master/samples/databases/northwind-pubs/instnwnd.sql instnwnd.sql
ADD sp_GetOrderSummary.sql /sql-setup/sp_GetOrderSummary.sql
ADD install-northwind.sh install-northwind.sh
USER root
RUN chown root:root install-northwind.sh && chmod +x install-northwind.sh
WORKDIR /entrypoint
ADD entrypoint.sh entrypoint.sh
RUN chown root:root entrypoint.sh && chmod +x entrypoint.sh
ENTRYPOINT "/entrypoint/entrypoint.sh"