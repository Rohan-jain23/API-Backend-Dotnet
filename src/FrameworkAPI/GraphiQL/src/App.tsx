import React, { useEffect, useMemo } from "react";
import { GraphiQL } from "graphiql";
import { createGraphiQLFetcher } from "@graphiql/toolkit";
import { explorerPlugin } from "@graphiql/plugin-explorer";
import "graphiql/graphiql.min.css";
import "@graphiql/plugin-explorer/dist/style.css";
import { useAuth } from "react-oidc-context";

const explorer = explorerPlugin({ showAttribution: false });

const App = () => {
  const { signinRedirect, isAuthenticated, isLoading, user } = useAuth();

  useEffect(() => {
      if (!isAuthenticated && !isLoading) {
        signinRedirect();
      }
  }, [isAuthenticated, isLoading]);

  const fetcher = useMemo(
    () =>
      user &&
      createGraphiQLFetcher({
        url: `https://${
          process.env.REACT_APP_BACKEND_ENDPOINT ?? window.location.host
        }/graphql`,
        subscriptionUrl: `wss://${
          process.env.REACT_APP_BACKEND_ENDPOINT ?? window.location.host
        }/graphql?access_token=${user.access_token}`,
        headers: {
          Authorization: `Bearer ${user.access_token}`,
        },
      }),
    [user?.access_token]
  );

  return (
    <>
      {!isAuthenticated && <div>{"unauthorized"}</div>}
      {fetcher && (
        <div className="graphiql-container-main">
          <GraphiQL fetcher={fetcher} plugins={[explorer]} />
        </div>
      )}
    </>
  );
};

export default App;
