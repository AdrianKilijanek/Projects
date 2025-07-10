 import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44393/',
  redirectUri: baseUrl,
  clientId: 'AKRek_App',
  responseType: 'code',
  scope: 'offline_access AKRek',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'AKRek',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44393',
      rootNamespace: 'AKRek',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
