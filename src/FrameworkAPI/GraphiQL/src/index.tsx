import React from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App";
import { AuthProvider } from "react-oidc-context";

const oidcConfig = {
  automaticSilentRenew: true,
  authority: `https://${
    process.env.REACT_APP_AUTH_HOSTNAME || window.location.host
  }/auth/realms/master`,
  redirect_uri: `https://${window.location.host}/wuh/graphiql`,
  post_logout_redirect_uri: `https://${
    process.env.REACT_APP_AUTH_HOSTNAME || window.location.host
  }/oauth/logout`,
  client_id: "isp",
  scope: "openid profile email offline_access",
  response_type: "code",
};

const root = createRoot(document.getElementById("root")!);
root.render(
  <AuthProvider {...oidcConfig}>
    <App />
  </AuthProvider>
);
