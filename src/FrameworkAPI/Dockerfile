FROM lx64ispentw.wuh-intern.de:4555/isp6/infrastructure/netcoreruntimeimage/netcoreruntimeimage:8.0-20

ARG DEVELOPMENT=true

LABEL com.wuh.loglevel=INFO
LABEL com.wuh.docker.memory=536870912
LABEL com.wuh.docker.cpus=1
LABEL com.wuh.development.docker.ports=5001:5001
RUN if [ "${DEVELOPMENT}" = "true" ] ; then echo Development is set. ; else echo Production is set. ; fi
LABEL com.wuh.development=$DEVELOPMENT

# Expose port for the Web API traffic
ENV ASPNETCORE_URLS http://+:5001
ENV ISDOCKER true
EXPOSE 5001

# Copy our code from the published "out" folder to the "/app" folder in our container
WORKDIR /app
COPY out .
COPY nlog.config .
COPY packages.lock.json .

HEALTHCHECK CMD netstat -an | grep --quiet 5001 || exit 1

# Build and run the dotnet application from within the container
ENTRYPOINT ["dotnet", "FrameworkAPI.dll"]
