import type { TicketType } from './ticket-type.enum';
import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateCustomerDto {
  name: string;
  surName: string;
  email: string;
  ticket: TicketType;
  pnumber: number;
}

export interface CustomerDto extends AuditedEntityDto<string> {
  name?: string;
  surname?: string;
  email?: string;
  ticket: TicketType;
  pnumber?: number;
}
