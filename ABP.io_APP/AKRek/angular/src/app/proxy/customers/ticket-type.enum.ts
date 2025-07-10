import { mapEnumToOptions } from '@abp/ng.core';

export enum TicketType {
  Undefined = 0,
  Ulgowy = 1,
  Normalny = 2,
  VIP = 3,
}

export const ticketTypeOptions = mapEnumToOptions(TicketType);
