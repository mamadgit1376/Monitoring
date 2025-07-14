// app/items/page.tsx
"use client";

import { useEffect, useState, useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  getItemsAction,
  getCompanyOptionsAction,
  addOrEditItemAction,
  deleteItemAction,
  assignCompaniesToItemAction,
  ItemModel,
  CompanyOption,
} from "./actions";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import MultipleSelector, { Option } from "@/components/ui/multiselect";
import { Label } from "@/components/ui/label";
import { Trash2, Edit, ChevronRight } from "lucide-react";
import Swal from "sweetalert2";

// Constants
const importanceOptions = [
  { label: "کم", value: 0 },
  { label: "متوسط", value: 1 },
  { label: "زیاد", value: 2 },
];

const initialItemFormState: ItemModel = {
  id: 0,
  itemName: "",
  repeatTimeMinute: 10,
  additionalUrlAddress: "",
  importanceLevel: 1,
  tblCategoryId: 1,
  companyIds: [],
};

// Submit Button Component
function SubmitButton({ isEditing }: { isEditing: boolean }) {
  const { pending } = useFormStatus();
  return (
    <button type="submit" className="btn btn-success" disabled={pending}>
      {pending ? "در حال ذخیره..." : isEditing ? "ویرایش" : "ثبت"}
    </button>
  );
}

// Main Page Component
export default function ItemsPage() {
  // State Management
  const [items, setItems] = useState<any>([]);
  const [companyOptions, setCompanyOptions] = useState<Option[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isItemModalOpen, setIsItemModalOpen] = useState(false);
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [currentItem, setCurrentItem] = useState<ItemModel>(initialItemFormState);
  const [selectedCompanyIds, setSelectedCompanyIds] = useState<string[]>([]);
  
  const [addEditState, formAction] = useActionState(addOrEditItemAction, undefined);

  // Data Fetching
  const loadInitialData = async () => {
    setIsLoading(true);
    const [itemsResult, companiesResult] = await Promise.all([
      getItemsAction(),
      getCompanyOptionsAction(),
    ]);

    if (itemsResult.success) {
      setItems(itemsResult.data?.data || []);
    } else {
      Swal.fire("خطا", "خطا در دریافت لیست آیتم‌ها", "error");
    }

    if (companiesResult.success) {
      setCompanyOptions(companiesResult.data?.data || []);
    } else {
      Swal.fire("خطا", "خطا در دریافت لیست شرکت‌ها", "error");
    }
    setIsLoading(false);
  };

  useEffect(() => {
    loadInitialData();
  }, []);

  // Effect for handling form submission result
  useEffect(() => {
    if (addEditState?.success) {
      Swal.fire("موفق!", `آیتم با موفقیت ${currentItem.id ? 'ویرایش' : 'ثبت'} شد.`, "success");
      setIsItemModalOpen(false);
      loadInitialData();
    } else if (addEditState?.error) {
      Swal.fire("خطا", addEditState.error.message, "error");
    }
  }, [addEditState]);

  // Event Handlers
  const handleOpenItemModal = (item?: ItemModel) => {
    setCurrentItem(item || initialItemFormState);
    setIsItemModalOpen(true);
  };

  const handleOpenAssignModal = (item: ItemModel) => {
    setCurrentItem(item);
    setSelectedCompanyIds(item.companyIds || []);
    setIsAssignModalOpen(true);
  };

  const handleDelete = async (item: ItemModel) => {
    const confirm = await Swal.fire({
      title: `آیا از حذف آیتم "${item.itemName}" اطمینان دارید؟`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'بله، حذف کن',
      cancelButtonText: 'انصراف'
    });

    if (confirm.isConfirmed) {
      const result = await deleteItemAction(item.id);
      if (result.success) {
        Swal.fire("موفق!", "آیتم با موفقیت حذف شد.", "success");
        loadInitialData();
      } else {
        Swal.fire("خطا", result.error?.message, "error");
      }
    }
  };
  
  const handleAssignSubmit = async () => {
    const result = await assignCompaniesToItemAction(currentItem.id, selectedCompanyIds);
     if (result.success) {
        Swal.fire("موفق!", "تخصیص شرکت‌ها با موفقیت انجام شد.", "success");
        setIsAssignModalOpen(false);
        loadInitialData();
      } else {
        Swal.fire("خطا", result.error?.message, "error");
      }
  }

  // Render Logic
  return (
    <div className="p-8 bg-base-300 min-h-screen">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">لیست آیتم‌ها</h1>
        <button className="btn btn-success" onClick={() => handleOpenItemModal()}>
          افزودن آیتم
        </button>
      </div>

      {isLoading ? <p className="text-center">در حال بارگذاری...</p> : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>عنوان</TableHead>
                <TableHead>تکرار (دقیقه)</TableHead>
                <TableHead>آدرس اضافی</TableHead>
                <TableHead>اهمیت</TableHead>
                <TableHead>شرکت‌ها</TableHead>
                <TableHead>تاریخ ایجاد</TableHead>
                <TableHead>وضعیت</TableHead>
                <TableHead className="text-center">عملیات</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} className={item.removed ? "opacity-50" : ""}>
                  <TableCell>{item.itemName}</TableCell>
                  <TableCell>{item.repeatTimeMinute}</TableCell>
                  <TableCell>{item.additionalUrlAddress || "-"}</TableCell>
                  <TableCell>
                    {importanceOptions.find(opt => opt.value === item.importanceLevel)?.label || "-"}
                  </TableCell>
                  <TableCell>
                     {item.companies?.join(' , ')}
                  </TableCell>
                  <TableCell>
                    {item.createDate ? new Date(item.createDate).toLocaleDateString("fa-IR") : "-"}
                  </TableCell>
                  <TableCell>
                    {item.removed && <span className="px-2 py-1 text-xs font-semibold text-white bg-red-600 rounded-full">حذف شده</span>}
                  </TableCell>
                  <TableCell className="flex justify-center items-center gap-2">
                    <button disabled={item.removed} className="btn btn-sm btn-primary" onClick={() => handleOpenAssignModal(item)} title="اختصاص شرکت‌ها">
                      <ChevronRight size={16} />
                    </button>
                    <button disabled={item.removed} className="btn btn-sm btn-warning" onClick={() => handleOpenItemModal(item)} title="ویرایش">
                      <Edit size={16} />
                    </button>
                    <button disabled={item.removed} className="btn btn-sm btn-error" onClick={() => handleDelete(item)} title="حذف">
                      <Trash2 size={16} />
                    </button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Add/Edit Item Modal */}
      {isItemModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50">
          <div className="bg-[#1f1f1f] p-8 rounded-2xl shadow-2xl w-full max-w-md">
            <h2 className="text-2xl mb-6 font-bold text-center">
              {currentItem.id ? "ویرایش آیتم" : "افزودن آیتم"}
            </h2>
            <form action={formAction} className="flex flex-col gap-5">
              <input type="hidden" name="oldId" value={currentItem.id || ""} />
              <input name="itemName" placeholder="عنوان آیتم" defaultValue={currentItem.itemName || ""} required className="input input-bordered bg-[#2a2a2a]" />
              <input name="repeatTimeMinute" type="number" placeholder="زمان تکرار" defaultValue={currentItem.repeatTimeMinute || 10} className="input input-bordered bg-[#2a2a2a]" />
              <input name="additionalUrlAddress" placeholder="آدرس اضافی" defaultValue={currentItem.additionalUrlAddress || ""} className="input input-bordered bg-[#2a2a2a]" />
              <select name="importanceLevel" defaultValue={currentItem.importanceLevel || 1} required className="select select-bordered bg-[#2a2a2a]">
                {importanceOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
              </select>
              <input name="tblCategoryId" type="number" placeholder="شناسه دسته‌بندی" defaultValue={currentItem.tblCategoryId || 1} required className="input input-bordered bg-[#2a2a2a]" />
              <div className="flex justify-between mt-6">
                <SubmitButton isEditing={!!currentItem.id} />
                <button type="button" onClick={() => setIsItemModalOpen(false)} className="btn btn-error">بستن</button>
              </div>
            </form>
          </div>
        </div>
      )}
      
       {/* Assign Companies Modal */}
      {isAssignModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50">
          <div className="bg-[#1f1f1f] text-white p-8 rounded-2xl shadow-2xl w-full max-w-md">
            <h2 className="text-2xl mb-6 font-bold text-center">اختصاص شرکت‌ها</h2>
            <div className="flex flex-col gap-4">
              <Label>انتخاب شرکت‌ها:</Label>
              <MultipleSelector
                value={companyOptions.filter(opt => selectedCompanyIds.includes(opt.value))}
                defaultOptions={companyOptions}
                onChange={(options) => setSelectedCompanyIds(options.map(opt => opt.value))}
                placeholder="انتخاب کنید..."
                emptyIndicator={<p className="text-center text-sm">شرکتی یافت نشد</p>}
              />
              <div className="flex justify-between mt-6">
                <button onClick={handleAssignSubmit} className="btn btn-success">ثبت</button>
                <button onClick={() => setIsAssignModalOpen(false)} className="btn btn-error">بستن</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
