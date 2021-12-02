import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { formatDateWithLocalTimezone } from '../_fn/date-function';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  maxDate: Date;
  serverValidationErrors: string[];

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
    this.initializeForm();
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      gender: ['male'],
      userName: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: [
        '',
        [Validators.required, Validators.minLength(4), Validators.maxLength(8)],
      ],
      confirmPassword: [
        '',
        [Validators.required, this.matchValues('password')],
      ],
    });
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      if (!control?.value) return null;
      return control?.value === control?.parent?.controls[matchTo]?.value
        ? null
        : { isMatching: true };
    };
  }

  register() {
    if (this.registerForm.invalid) return;
    const data = Object.assign({}, this.registerForm.value);

    if (data.dateOfBirth)
      data.dateOfBirth = formatDateWithLocalTimezone(data.dateOfBirth);
    else data.dateOfBirth = undefined;

    this.accountService.register(data).subscribe(
      () => {
        this.serverValidationErrors = null;
        this.router.navigateByUrl('/members');
      },
      (response) => {
        if (response.join) {
          this.serverValidationErrors = response;
        }
      }
    );
  }
}
