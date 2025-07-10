import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, NgModule, OnInit } from '@angular/core';
import { CustomerService, CustomerDto, ticketTypeOptions } from '@proxy/customers';
import { FormGroup, FormBuilder, Validators, AbstractControl, AsyncValidatorFn } from '@angular/forms'; 
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationService, Confirmation, DEFAULT_VALIDATION_BLUEPRINTS } from '@abp/ng.theme.shared';




@Component({
  selector: 'app-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.scss'],
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class CustomerComponent implements OnInit {
  customer = { items: [], totalCount: 0 } as PagedResultDto<CustomerDto>;

  selectedCustomer = {} as CustomerDto;

  form: FormGroup; 
  ticketTypes = ticketTypeOptions;

  isModalOpen = false;

  constructor(
    public readonly list: ListService,
    private customerService: CustomerService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit() {
    const customerStreamCreator = (query) => this.customerService.getList(query);

    this.list.hookToQuery(customerStreamCreator).subscribe((response) => {
      this.customer = response;
    });
  }

  createCustomer() {
    this.selectedCustomer = {} as CustomerDto;
    this.buildForm(); 
    this.isModalOpen = true;
  }

  editCustomer(id: string) {
    this.customerService.get(id).subscribe((customer) => {
      this.selectedCustomer = customer;
      this.buildForm();
      this.isModalOpen = true;
    });
  }


  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedCustomer.name || '', {validators: [Validators.required, Validators.pattern]}],
      surname: [this.selectedCustomer.surname || '', {validators: [Validators.required, Validators.pattern]}],
      email: [this.selectedCustomer.email || '', {validators: [Validators.required, Validators.email]}],
      pnumber: [this.selectedCustomer.pnumber || null, {validators: [Validators.required, Validators.min(100000000), Validators.max(999999999)]}],
      ticket: [this.selectedCustomer.ticket || null, Validators.required],
      
    });
  }

  
    save() {
      if (this.form.invalid) {
        return;
      }
  
      const request = this.selectedCustomer.id
      ? this.customerService.update(this.selectedCustomer.id, this.form.value)
      : this.customerService.create(this.form.value);

      request.subscribe(() => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
      });
    }

    
    delete(id: string) {
      this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.customerService.delete(id).subscribe(() => this.list.get());
        }
      });
    }
}
