import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { Observable, Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root',
})
export class ConfirmService {
  modalRef: BsModalRef;

  constructor(private modalService: BsModalService) {}

  confirm(
    message: string,
    title = 'Confirmation',
    btnOkText = 'Ok',
    btnCancelText = 'Cancel'
  ): Observable<boolean> {
    const config: ModalOptions = {
      initialState: {
        message,
        title,
        btnOkText,
        btnCancelText,
      },
    };

    this.modalRef = this.modalService.show(ConfirmDialogComponent, config);

    const subject = new Subject<boolean>();
    this.modalService.onHidden.pipe(take(1)).subscribe(() => {
      subject.next(this.modalRef.content.result);
      subject.complete();
    });
    return subject.asObservable();
  }
}
