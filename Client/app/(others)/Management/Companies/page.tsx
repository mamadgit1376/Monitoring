// app/companies/page.tsx
"use client";

import { useEffect, useState, useRef, useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  getCompaniesAction,
  addOrEditCompanyAction,
  deleteCompanyAction,
  CompanyModel,
} from "./actions"; // Actions are imported from the separate server file
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Trash2, Edit } from "lucide-react";
import Swal from "sweetalert2";

// A dedicated component for the submit button that shows a pending state.
// It must be a direct child of the <form> to access the form's status.
function SubmitButton({ isEdit }: { isEdit: boolean }) {
  const { pending } = useFormStatus();
  return (
    <button type="submit" className="btn btn-success" disabled={pending}>
      {pending ? "در حال ذخیره..." : isEdit ? "ویرایش" : "افزودن"}
    </button>
  );
}

// This is the initial state for the form when adding a new company.
const initialFormState: CompanyModel = {
  id: 0, // Use 0 or another indicator for a new entry
  companyName: "",
  baseUrlAddress: "",
  locationAddress: "",
  nationalCode: "",
  apiUser: "",
  apiPassword: "",
};

export default function CompaniesPage() {
  // State for the list of companies displayed in the table
  const [companies, setCompanies] = useState<CompanyModel[]>([]);
  // State to show a loading indicator while fetching data
  const [isLoading, setIsLoading] = useState(true);
  // State to control the visibility of the add/edit modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  // State to hold the data of the company being added or edited
  const [formState, setFormState] = useState<CompanyModel>(initialFormState);

  // Hook to connect the form to the server action and manage its state
  const [actionResult, formAction] = useActionState(addOrEditCompanyAction, undefined);

  // Function to fetch the list of companies from the server
  const loadCompanies = async () => {
    setIsLoading(true);
    const result = await getCompaniesAction();
    if (result.success) {
      setCompanies(result.data.data || []);
    } else {
      Swal.fire("خطا", result.error?.message || "خطا در دریافت اطلاعات", "error");
    }
    setIsLoading(false);
  };

  // Fetch data when the component first loads
  useEffect(() => {
    loadCompanies();
  }, []);

  // This effect runs when the server action (add/edit) completes
  useEffect(() => {
    if (actionResult?.success) {
      const message = formState.id ? "شرکت با موفقیت ویرایش شد." : "شرکت با موفقیت افزوده شد.";
      Swal.fire("موفق!", message, "success");
      setIsModalOpen(false);
      loadCompanies(); // Reload the list to show the changes
    } else if (actionResult && actionResult.error) {
      Swal.fire("خطا", actionResult.error.message, "error");
    }
  }, [actionResult]);

  // Handler to open the modal for adding a new company
  const handleAddNew = () => {
    setFormState(initialFormState);
    setIsModalOpen(true);
  };

  // Handler to open the modal for editing an existing company
  const handleEdit = (company: CompanyModel) => {
    setFormState(company);
    setIsModalOpen(true);
  };

  // Handler to delete a company
  const handleDelete = async (company: CompanyModel) => {
    const confirmResult = await Swal.fire({
      title: `آیا از غیرفعال کردن شرکت "${company.companyName}" مطمئن هستید؟`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "بله، غیرفعال کن",
      cancelButtonText: "انصراف",
    });

    if (confirmResult.isConfirmed) {
      // Call the delete server action
      const result = await deleteCompanyAction(company.id);
      if (result.success) {
        Swal.fire("موفق!", "شرکت با موفقیت غیرفعال شد.", "success");
        loadCompanies(); // Reload the list
      } else {
        Swal.fire("خطا", result.error?.message || "عملیات ناموفق بود", "error");
      }
    }
  };

  return (
    <div className="p-8 bg-base-300 min-h-screen">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">لیست شرکت‌ها</h1>
        <button className="btn btn-success" onClick={handleAddNew}>
          افزودن شرکت
        </button>
      </div>

      {isLoading ? (
        <p className="text-center">در حال بارگذاری...</p>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>نام شرکت</TableHead>
                <TableHead>آدرس اصلی</TableHead>
                <TableHead>شناسه ملی</TableHead>
                <TableHead>عملیات</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {companies.map((comp) => (
                <TableRow key={comp.id}>
                  <TableCell>{comp.companyName}</TableCell>
                  <TableCell>{comp.baseUrlAddress}</TableCell>
                  <TableCell>{comp.nationalCode}</TableCell>
                  <TableCell className="flex gap-2">
                    <button className="btn btn-sm btn-warning text-warning-content" onClick={() => handleEdit(comp)}>
                      <Edit size={16} />
                    </button>
                    <button className="btn btn-sm btn-error text-error-content" onClick={() => handleDelete(comp)}>
                      <Trash2 size={16} />
                    </button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Modal for Adding/Editing a Company */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50">
          <div className="bg-[#1f1f1f] p-8 rounded-2xl shadow-2xl w-full max-w-md">
            <h2 className="text-2xl mb-6 font-bold text-center">
              {formState.id ? "ویرایش شرکت" : "افزودن شرکت"}
            </h2>
            <form action={formAction} className="flex flex-col gap-5">
              {/* Hidden fields to pass ID and edit status to the server action */}
              <input type="hidden" name="id" value={formState.id || ""} />
              <input type="hidden" name="isEdit" value={String(!!formState.id)} />

              <input name="companyName" placeholder="نام شرکت" defaultValue={formState.companyName} required className="input input-bordered bg-[#2a2a2a]" />
              <input name="baseUrlAddress" placeholder="آدرس اصلی" defaultValue={formState.baseUrlAddress} required className="input input-bordered bg-[#2a2a2a]" />
              <input name="locationAddress" placeholder="آدرس محل شرکت" defaultValue={formState.locationAddress} required className="input input-bordered bg-[#2a2a2a]" />
              <input name="nationalCode" placeholder="شناسه ملی" defaultValue={formState.nationalCode} required maxLength={20} className="input input-bordered bg-[#2a2a2a]" />
              <input name="apiUser" placeholder="کاربر API" defaultValue={formState.apiUser} required className="input input-bordered bg-[#2a2a2a]" />
              <input name="apiPassword" type="password" placeholder="رمز عبور API (در صورت تغییر وارد شود)" className="input input-bordered bg-[#2a2a2a]" />
              
              <div className="flex justify-between mt-6">
                <SubmitButton isEdit={!!formState.id} />
                <button type="button" onClick={() => setIsModalOpen(false)} className="btn btn-error">
                  بستن
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
